use chrono::Utc;
use tauri::{
    webview::PageLoadEvent, AppHandle, Emitter, Manager, WebviewUrl, WebviewWindowBuilder,
    WindowEvent,
};
use url::Url;

use crate::{
    db::sessions as sessions_db, error::AppResult, models::ExportErrorEvent, state::AppState,
};

pub const AUTH_WINDOW_LABEL_PREFIX: &str = "auth-export";
const CHROME_USER_AGENT: &str = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 \
                                 (KHTML, like Gecko) Chrome/124.0.0.0 Safari/537.36";
const SCRAPE_SCRIPT_TEMPLATE: &str = include_str!("scripts/scraper.js");

pub fn auth_window_label(session_id: &str) -> String {
    format!("{AUTH_WINDOW_LABEL_PREFIX}-{session_id}")
}

fn build_scrape_script(session_id: &str, export_images: bool) -> String {
    SCRAPE_SCRIPT_TEMPLATE
        .replace("__SESSION_ID__", session_id)
        .replace(
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
    let notes_url = Url::parse(&format!("https://{domain}/note/h5/?_locale=en-US"))?;

    let window_label = auth_window_label(session_id);
    let session_id_for_close = session_id.to_string();
    let script = build_scrape_script(session_id, export_images);

    let window = WebviewWindowBuilder::new(app, &window_label, WebviewUrl::External(notes_url))
        .title("Mi Cloud Shell")
        .inner_size(1200.0, 860.0)
        .closable(true)
        .user_agent(CHROME_USER_AGENT)
        .on_page_load(move |window, payload| {
            if payload.event() != PageLoadEvent::Finished {
                return;
            }

            if payload.url().path().starts_with("/note/h5") {
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
                Some("Mi Cloud access window closed by user."),
            );
            let _ = app_handle.emit(
                "export:error",
                ExportErrorEvent {
                    session_id: active.session_id,
                    message: "Mi Cloud access window closed by user.".to_string(),
                },
            );
        }
    });

    Ok(window_label)
}
