(() => {
  const state = window.__xiaomiNoteExporterState || {
    running: false,
    completed: false,
    lastError: "",
  };
  window.__xiaomiNoteExporterState = state;

  if (state.completed || state.running) {
    return;
  }
  state.running = true;
  state.lastError = "";

  const sessionId = "__SESSION_ID__";
  const exportImages = __EXPORT_IMAGES__;

  const sleep = (ms) => new Promise((resolve) => setTimeout(resolve, ms));

  const resolveInvoke = async (timeoutMs = 30000) => {
    const started = Date.now();
    while (Date.now() - started < timeoutMs) {
      if (
        window.__TAURI__ &&
        window.__TAURI__.core &&
        typeof window.__TAURI__.core.invoke === "function"
      ) {
        return (cmd, payload) => window.__TAURI__.core.invoke(cmd, payload);
      }
      if (
        window.__TAURI_INTERNALS__ &&
        typeof window.__TAURI_INTERNALS__.invoke === "function"
      ) {
        return (cmd, payload) =>
          window.__TAURI_INTERNALS__.invoke(cmd, payload);
      }
      await sleep(100);
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

  const classNameIncludes = (element, fragment) => {
    if (!element || !fragment) return false;
    const cls =
      typeof element.className === "string"
        ? element.className
        : String(element.className || "");
    return cls.includes(fragment);
  };

  const getCards = (container) => {
    if (!container) return [];
    return Array.from(container.querySelectorAll("div"))
      .filter((node) => node instanceof HTMLElement)
      .filter((node) => {
        const cls =
          typeof node.className === "string"
            ? node.className
            : String(node.className || "");
        return cls.split(/\s+/).some((token) => token.startsWith("note-item"));
      });
  };

  const findOpenCard = (container, cards) => {
    const openMarker = container.querySelector("div[class*='open']");
    if (openMarker instanceof HTMLElement) {
      const owner = cards.find(
        (card) => card === openMarker || card.contains(openMarker),
      );
      if (owner) {
        return owner;
      }
    }
    return cards.find((card) => classNameIncludes(card, "open")) || null;
  };

  const resolveTargetCard = (openCard, cards, isFirstIteration) => {
    if (!cards.length) return null;

    if (isFirstIteration) {
      return openCard || cards[0];
    }

    if (!openCard) {
      return cards[0];
    }

    const openIndex = cards.indexOf(openCard);
    if (openIndex < 0) {
      return cards[0];
    }

    if (openIndex + 1 < cards.length) {
      return cards[openIndex + 1];
    }

    return null;
  };

  const isCardOpen = (card) => {
    if (!(card instanceof HTMLElement)) return false;
    if (classNameIncludes(card, "open")) return true;
    return !!card.querySelector("div[class*='open']");
  };

  const extractCreatedString = (card) => {
    if (!card) return "";
    const candidate = card.querySelector("div:nth-child(2) div:nth-child(1)");
    return (candidate?.textContent || "").trim();
  };

  const getImageSrc = (img) => {
    if (!(img instanceof HTMLImageElement)) return "";
    return img.currentSrc || img.src || "";
  };

  const isRealImageLoaded = (img) => {
    if (!(img instanceof HTMLImageElement)) return false;
    const src = getImageSrc(img);
    if (!src || src.startsWith("data:image/svg+xml")) return false;
    return img.complete === true && (img.naturalWidth || 0) > 0;
  };

  const waitForImageReady = async (img, timeoutMs = 3000) => {
    const started = Date.now();
    while (Date.now() - started < timeoutMs) {
      if (isRealImageLoaded(img)) {
        return true;
      }
      await sleep(120);
    }
    return isRealImageLoaded(img);
  };

  const waitUntilImagesAreReady = async (images, perItemTimeoutMs = 3000) => {
    for (const img of images) {
      const ok = await waitForImageReady(img, perItemTimeoutMs);
      if (!ok) {
        return false;
      }
    }
    return true;
  };

  const collectImages = async (container, createdString) => {
    if (!exportImages || !container) {
      return [];
    }

    const images = [];
    const initialImageNodes = Array.from(
      container.querySelectorAll(".image-view img"),
    );
    if (initialImageNodes.length > 0) {
      const allLoaded = await waitUntilImagesAreReady(initialImageNodes, 3000);
      if (!allLoaded) {
        console.warn(
          "Some images did not finish loading in time, proceeding with what is available",
          {
            initialCount: initialImageNodes.length,
            loadedCount: initialImageNodes.filter(isRealImageLoaded).length,
          },
        );
      }
    }

    const imageNodes = Array.from(
      container.querySelectorAll(".image-view img"),
    );
    let index = 0;

    for (const img of imageNodes) {
      if (!isRealImageLoaded(img)) {
        const ready = await waitForImageReady(img, 1500);
        if (!ready) {
          index += 1;
          continue;
        }
      }

      const src = getImageSrc(img);
      if (!src || src.startsWith("data:")) {
        continue;
      }

      try {
        const response = await fetch(src, { credentials: "true" });
        if (!response.ok) {
          continue;
        }

        const arrayBuffer = await response.arrayBuffer();
        const base64 = toBase64(arrayBuffer);
        const sanitizedCreated =
          (createdString || "").replace(/[^0-9a-zA-Z_-]+/g, "_") || "unknown";
        images.push({
          name: `note_img_${index}_${sanitizedCreated}.png`,
          sourceUrl: src,
          dataBase64: base64,
        });
      } catch (e) {
        console.error("Failed to fetch image", { src }, e);
      }

      index += 1;
    }

    return images;
  };

  const run = async () => {
    let invoke = null;

    try {
      invoke = await resolveInvoke();

      const ready = await waitFor(() => {
        const spinner = document.querySelector("div[id*='spinner']");
        const spinnerHidden =
          !spinner || window.getComputedStyle(spinner).display === "none";
        const createButton = document.querySelector(
          "button[class*='btn-create']",
        );
        return spinnerHidden && !!createButton;
      }, 300000);

      if (!ready) {
        throw new Error(
          "Timeout waiting for Xiaomi Notes page. Sign in might be incomplete.",
        );
      }

      const listContainer = document.querySelector(
        "div[class*='note-list-items']",
      );
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

      let processed = 0;
      let guard = 0;
      let isFirstIteration = true;
      let noTargetRetries = 0;

      while (processed < total && guard < total * 16) {
        const cards = getCards(listContainer);
        const openCard = findOpenCard(listContainer, cards);
        const target = resolveTargetCard(openCard, cards, isFirstIteration);

        if (!target) {
          noTargetRetries += 1;
          const waitMs = Math.min(1200, 200 + noTargetRetries * 150);
          await sleep(waitMs);

          if (noTargetRetries < 4) {
            continue;
          }

          const fallbackHeight =
            cards.length > 0
              ? Math.max(
                  Math.floor(
                    cards[cards.length - 1].getBoundingClientRect().height *
                      0.8,
                  ),
                  96,
                )
              : Math.max(Math.floor(listContainer.clientHeight * 0.25), 96);

          listContainer.scrollBy(0, fallbackHeight);
          noTargetRetries = 0;
          await sleep(300);
          guard += 1;
          continue;
        }
        noTargetRetries = 0;

        if (isFirstIteration) {
          isFirstIteration = false;
        }

        target.click();
        const opened = await waitFor(() => isCardOpen(target), 5000, 120);
        if (!opened) {
          await sleep(250);
          continue;
        }
        await sleep(240);

        const createdString = extractCreatedString(target);
        const contentReady = await waitFor(
          () => !!document.querySelector("div[class*='pm-container']"),
          5000,
          120,
        );
        const titleReady = await waitFor(
          () => !!document.querySelector("div[class*='origin-title'] > div"),
          3000,
          120,
        );

        const titleNode = titleReady
          ? document.querySelector("div[class*='origin-title'] > div")
          : null;
        const noteContainer = contentReady
          ? document.querySelector("div[class*='pm-container']")
          : null;

        const unsupported = !noteContainer;
        const title = (titleNode?.textContent || "").trim();
        const content = unsupported
          ? ""
          : (noteContainer.innerText || "").trim();
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
        listContainer.scrollBy(0, target.getBoundingClientRect().height);
        await sleep(420);
      }

      await invoke("finish_scrape", { sessionId });
      state.completed = true;
      state.lastError = "";
    } catch (error) {
      const message = error instanceof Error ? error.message : String(error);
      state.lastError = message;
      console.error("Scrape failed", { message });
      try {
        if (invoke) {
          await invoke("fail_scrape", { sessionId, message });
        } else {
          const lateInvoke = await resolveInvoke(2000);
          await lateInvoke("fail_scrape", { sessionId, message });
        }
      } catch (_) {}
    } finally {
      state.running = false;
    }
  };

  void run();
})();
