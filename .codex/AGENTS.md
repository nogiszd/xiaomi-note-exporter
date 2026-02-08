## Xiaomi Note Exporter (Tauri + Vue)

### Purpose

- Desktop app that exports Xiaomi/Mi Cloud notes to Markdown, with optional image downloads.
- Supports browsing past export sessions, viewing/editing exported files, and converting Markdown exports to JSON.

### Tech Stack

- Frontend: Vue 3 + TypeScript + Vite, Pinia, Vue Router (hash).
- UI: Tailwind CSS v4, shadcn-vue components, md-editor-v3.
- Backend: Tauri v2 (Rust), SQLite (rusqlite), reqwest, chrono.

### How It Works (High Level)

1. User starts an export from the Export view.
2. Tauri opens a Mi Cloud auth window and injects a scraping script.
3. Scraper reports progress/events back to the UI via Tauri events.
4. Notes are written to Markdown (split files or one combined file) and images saved optionally.
5. Session metadata is stored in SQLite for history and viewer features.

### Frontend Structure

- App shell: [src/App.vue](src/App.vue)
  - Renders AppHeader/AppSidebar and RouterView.
  - Initializes export event listeners on mount.
- Routing: [src/router/index.ts](src/router/index.ts)
  - Routes: Export, History, Converter, Viewer, Settings.
- API bridge: [src/lib/api.ts](src/lib/api.ts)
  - Invokes Tauri commands (export, sessions, files, settings, updates, converter).
- State stores:
  - Export: [src/stores/export.ts](src/stores/export.ts) (progress, events, logs).
  - Sessions: [src/stores/sessions.ts](src/stores/sessions.ts) (history list).
  - Settings: [src/stores/settings.ts](src/stores/settings.ts) (theme, default export dir).
- Views:
  - Export: [src/views/export-view.vue](src/views/export-view.vue)
  - History: [src/views/history-view.vue](src/views/history-view.vue)
  - Viewer: [src/views/viewer-view.vue](src/views/viewer-view.vue)
  - Converter: [src/views/converter-view.vue](src/views/converter-view.vue)
  - Settings: [src/views/settings-view.vue](src/views/settings-view.vue)

### Backend Structure (Tauri)

- Entry point: [src-tauri/src/main.rs](src-tauri/src/main.rs)
- App bootstrap: [src-tauri/src/lib.rs](src-tauri/src/lib.rs)
  - Creates app data dirs, sessions DB, settings file.
  - Registers commands and plugins (dialog, opener).
- State: [src-tauri/src/state.rs](src-tauri/src/state.rs)
  - Tracks current export session and metadata.
- Models: [src-tauri/src/models.rs](src-tauri/src/models.rs)
  - Session, settings, event payloads, file entries.

### Tauri Commands (JS invoke names)

- Export flow: `start_export`, `cancel_export`, `report_export_total`, `append_scraped_note`,
  `finish_scrape`, `fail_scrape`, `download_scrape_image`.
- Files: `read_export_file`, `write_export_file`, `list_export_files`, `open_in_explorer`.
- Sessions: `get_sessions`, `get_session`, `delete_session`.
- Converter: `convert_to_json`.
- Settings: `get_app_settings`, `update_app_settings`.
- Updates: `check_latest_release_version`.

### Tauri Events Emitted to Frontend

- `export:progress` (ExportProgressEvent)
- `export:complete` (ExportCompleteEvent)
- `export:error` (ExportErrorEvent)

### Export Details

- Output paths are timestamped:
  - Split mode: folder `exported_notes_<stamp>/` with note files.
  - Single file mode: `exported_notes_<stamp>.md`.
- Optional images saved to `images_<stamp>/` or `exported_notes_<stamp>/images/`.
- Supported image hosts are allowlisted in Rust (Mi Cloud domains only).

### Data Storage

- SQLite DB at app data dir: `sessions.db`.
- Settings JSON at app data dir: `settings.json`.
- Migrations: [src-tauri/src/db/migrations.rs](src-tauri/src/db/migrations.rs).

### Markdown Formatting

- Notes are written via [src-tauri/src/services/markdown.rs](src-tauri/src/services/markdown.rs).
- Title falls back to "Untitled Note".
- Created date is appended as `*Created at: dd/MM/YYYY HH:mm*`.

### Converter

- Converts exported Markdown into JSON via `convert_to_json` (Rust service).
- UI surface in [src/views/converter-view.vue](src/views/converter-view.vue).

### Build and Run

- Frontend dev: `npm run dev`
- Frontend build: `npm run build`
- Tauri dev: `npm run tauri dev`
- Tauri build: `npm run tauri build`

### Key Config Files

- Vite: [vite.config.ts](vite.config.ts)
- Tauri: [src-tauri/tauri.conf.json](src-tauri/tauri.conf.json)
- Package deps: [package.json](package.json)
- Rust deps: [src-tauri/Cargo.toml](src-tauri/Cargo.toml)

### Notes for Contributors

- Export workflow depends on the embedded scraper script: [src-tauri/src/services/scripts/scraper.js](src-tauri/src/services/scripts/scraper.js)
- Theme is applied from settings on boot in [src/stores/settings.ts](src/stores/settings.ts).
- Viewer uses `md-editor-v3` and writes edits back via `write_export_file`.
