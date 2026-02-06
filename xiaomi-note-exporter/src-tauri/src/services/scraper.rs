use chrono::Utc;
use tauri::{AppHandle, Emitter, Manager, WebviewUrl, WebviewWindowBuilder, WindowEvent};
use url::Url;

use crate::{
    db::sessions as sessions_db, error::AppResult, models::ExportErrorEvent, state::AppState,
};

pub const AUTH_WINDOW_LABEL_PREFIX: &str = "auth-export";
const CHROME_USER_AGENT: &str = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 \
                                 (KHTML, like Gecko) Chrome/124.0.0.0 Safari/537.36";

pub fn auth_window_label(session_id: &str) -> String {
    format!("{AUTH_WINDOW_LABEL_PREFIX}-{session_id}")
}

fn build_scrape_script(session_id: &str, export_images: bool) -> String {
    let script = r#"
(() => {
  if (window.__xiaomiExporterRunning) {
    return;
  }
  window.__xiaomiExporterRunning = true;

  const sessionId = "__SESSION_ID__";
  const exportImages = __EXPORT_IMAGES__;

  const sleep = (ms) => new Promise((resolve) => setTimeout(resolve, ms));

  const invoke = async (cmd, payload) => {
    if (window.__TAURI__ && window.__TAURI__.core && typeof window.__TAURI__.core.invoke === "function") {
      return window.__TAURI__.core.invoke(cmd, payload);
    }
    if (window.__TAURI_INTERNALS__ && typeof window.__TAURI_INTERNALS__.invoke === "function") {
      return window.__TAURI_INTERNALS__.invoke(cmd, payload);
    }
    throw new Error("Tauri invoke bridge unavailable");
  };

  const waitFor = async (predicate, timeoutMs, intervalMs = 250) => {
    const started = Date.now();
    while (Date.now() - started < timeoutMs) {
      if (predicate()) {
        return true;
      }
      await sleep(intervalMs);
    }
    return false;
  };

  const toBase64 = (arrayBuffer) => {
    const bytes = new Uint8Array(arrayBuffer);
    const chunkSize = 0x8000;
    let binary = "";
    for (let i = 0; i < bytes.length; i += chunkSize) {
      const chunk = bytes.subarray(i, i + chunkSize);
      binary += String.fromCharCode.apply(null, chunk);
    }
    return btoa(binary);
  };

  const parseTotal = () => {
    const totalNode = document.querySelector("div[class*='note-count-select']");
    if (!totalNode) return 0;
    const digits = (totalNode.textContent || "").replace(/\D/g, "");
    const parsed = Number.parseInt(digits, 10);
    return Number.isFinite(parsed) ? parsed : 0;
  };

  const getCards = (container) => {
    if (!container) return [];
    return Array.from(container.children).filter((node) => node instanceof HTMLElement);
  };

  const extractCreatedString = (card) => {
    if (!card) return "";
    const candidate = card.querySelector("div:nth-child(2) div:nth-child(1)");
    return (candidate?.textContent || "").trim();
  };

  const collectImages = async (container, createdString) => {
    if (!exportImages || !container) {
      return [];
    }

    const images = [];
    const imageNodes = Array.from(container.querySelectorAll(".image-view img"));
    let index = 0;

    for (const img of imageNodes) {
      const src = img.currentSrc || img.src || "";
      if (!src || src.startsWith("data:")) {
        continue;
      }

      try {
        const response = await fetch(src, { credentials: "include" });
        if (!response.ok) {
          continue;
        }

        const arrayBuffer = await response.arrayBuffer();
        const base64 = toBase64(arrayBuffer);
        const sanitizedCreated = (createdString || "").replace(/[^0-9a-zA-Z_-]+/g, "_") || "unknown";
        images.push({
          name: `note_img_${index}_${sanitizedCreated}.png`,
          sourceUrl: src,
          dataBase64: base64,
        });
      } catch (_) {
      }

      index += 1;
    }

    return images;
  };

  const run = async () => {
    try {
      const ready = await waitFor(() => {
        const spinner = document.querySelector("div[class*='spinner']");
        const spinnerHidden = !spinner || window.getComputedStyle(spinner).display === "none";
        const createButton = document.querySelector("button[class*='btn-create']");
        return spinnerHidden && !!createButton;
      }, 300000);

      if (!ready) {
        throw new Error("Timeout waiting for Xiaomi Notes page. Sign in might be incomplete.");
      }

      const listContainer = document.querySelector("div[class*='note-list-items']");
      if (!listContainer) {
        throw new Error("Could not find note list container.");
      }

      let total = parseTotal();
      if (total <= 0) {
        total = getCards(listContainer).length;
      }
      if (total <= 0) {
        throw new Error("No notes found for export.");
      }

      await invoke("report_export_total", { sessionId, total });

      const seen = new Set();
      let processed = 0;
      let guard = 0;

      while (processed < total && guard < total * 8) {
        const cards = getCards(listContainer);
        let target = null;

        for (const card of cards) {
          const marker = card.dataset.xiaomiExporterSeen || "";
          if (marker === "1") {
            continue;
          }
          target = card;
          break;
        }

        if (!target) {
          listContainer.scrollBy(0, Math.max(240, listContainer.clientHeight / 2));
          await sleep(250);
          guard += 1;
          continue;
        }

        target.dataset.xiaomiExporterSeen = "1";
        target.click();
        await sleep(400);

        const createdString = extractCreatedString(target);
        const titleNode = document.querySelector("div[class*='origin-title'] > div");
        const noteContainer = document.querySelector("div[class*='pm-container']");
        const unsupported = !noteContainer;
        const title = (titleNode?.textContent || "").trim();
        const content = unsupported ? "" : ((noteContainer.innerText || "").trim());
        const images = await collectImages(noteContainer, createdString);

        await invoke("append_scraped_note", {
          sessionId,
          note: {
            title,
            content,
            createdString,
            unsupported,
            images,
          },
        });

        processed += 1;
        listContainer.scrollBy(0, Math.max(target.getBoundingClientRect().height, 120));
        await sleep(200);
      }

      await invoke("finish_scrape", { sessionId });
    } catch (error) {
      const message = error instanceof Error ? error.message : String(error);
      try {
        await invoke("fail_scrape", { sessionId, message });
      } catch (_) {
      }
    }
  };

  void run();
})();
"#;

    script.replace("__SESSION_ID__", session_id).replace(
        "__EXPORT_IMAGES__",
        if export_images { "true" } else { "false" },
    )
}

pub fn create_auth_window(
    app: &AppHandle,
    session_id: &str,
    domain: &str,
    export_images: bool,
) -> AppResult<String> {
    let notes_url = format!("https://{domain}/note/h5/?_locale=en-US");
    let mut login_url = Url::parse("https://account.xiaomi.com/pass/serviceLogin")?;
    login_url
        .query_pairs_mut()
        .append_pair("sid", "i.mi.com")
        .append_pair("_locale", "en_US")
        .append_pair("continue", &notes_url);

    let window_label = auth_window_label(session_id);
    let session_id_for_close = session_id.to_string();
    let script = build_scrape_script(session_id, export_images);

    let window = WebviewWindowBuilder::new(app, &window_label, WebviewUrl::External(login_url))
        .title("Mi Cloud Sign-In")
        .inner_size(1200.0, 860.0)
        .closable(true)
        .user_agent(CHROME_USER_AGENT)
        .on_page_load(move |window, payload| {
            let current_url = payload.url().to_string();
            if current_url.contains("/note/h5/") {
                let _ = window.eval(script.clone());
            }
        })
        .build()?;

    let app_handle = app.clone();
    window.on_window_event(move |event| {
        if !matches!(event, WindowEvent::CloseRequested { .. }) {
            return;
        }

        let state = app_handle.state::<AppState>();
        let active_export = {
            let mut guard = match state.active_export.lock() {
                Ok(guard) => guard,
                Err(_) => return,
            };

            if guard
                .as_ref()
                .map(|active| active.session_id.as_str() == session_id_for_close.as_str())
                .unwrap_or(false)
            {
                guard.take()
            } else {
                None
            }
        };

        if let Some(active) = active_export {
            let _ = sessions_db::set_session_outcome(
                &state.db_path,
                &active.session_id,
                "cancelled",
                &Utc::now().to_rfc3339(),
                active.notes_count,
                active.images_count,
                Some("Authentication window closed by user."),
            );
            let _ = app_handle.emit(
                "export:error",
                ExportErrorEvent {
                    session_id: active.session_id,
                    message: "Authentication window closed by user.".to_string(),
                },
            );
        }
    });

    Ok(window_label)
}
