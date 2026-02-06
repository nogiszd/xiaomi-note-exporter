# PLANS.md - Xiaomi Note Exporter (Tauri Rewrite)

This file contains the detailed product and technical specs for the rewrite.

## Project Overview

Xiaomi Note Exporter is a desktop app that exports notes from Mi Cloud into Markdown files (including embedded images). Users sign in to their Mi Cloud account, and the app scrapes all notes and saves them locally.

This directory (xiaomi-note-exporter/) is a full rewrite of the original C#/.NET 8 console application (in ../xiaomiNoteExporter/) using Tauri with a Vue 3 + TypeScript frontend and a Rust backend.

## Original App Behavior (C# reference)

The original app (v1.7.6) is a CLI tool built with .NET 8, Selenium WebDriver, and Pastel. It performs the following:

Core Flow

1. Launch and configuration: user optionally provides a Mi Cloud domain (default us.i.mi.com) and CLI flags.
2. Browser automation: a headless-capable Chrome instance navigates to https://{domain}/note/h5/?\_locale=en-US.
3. Manual sign-in: the user signs in via the browser window; the app waits for a key press.
4. Scraping: iterate through every note in the sidebar list.
   - Click each note to open it.
   - Extract title, body text, creation date from the DOM.
   - Scroll the note list to reveal the next item.
   - Handle unsupported note types (Mind-map, Sound) by logging a placeholder.
5. Markdown export: notes are written to .md files with a \*\*\*\* separator between notes (or split into individual files).
6. Image export: embedded images (img inside .image-view) are downloaded via authenticated HTTP requests and saved to an images\_\* directory.
7. JSON conversion: a secondary mode (-j) converts previously exported .md files into JSON using NoteDto.

CLI Flags

- -h, --help: show help and exit
- -d, --domain <domain>: set Mi Cloud domain (default us.i.mi.com)
- -s, --split <format>: split notes into individual files with custom timestamp format
- -di, --disable-images: skip downloading embedded images
- -j, --json <path>: convert exported .md file(s) to JSON
- -md, --manual-driver: use local chromedriver.exe instead of Selenium Manager

Date Parsing

- Relative times: "5 minutes ago", "2 hours ago" (RelativeTimeParser)
- "now" / "yesterday" special cases
- Simplified dates: "M/d HH:mm" without year (SimplifiedDateParser)
- Full date formats: dd/MM/yyyy HH:mm, M/d/yyyy h:mm tt, etc.

Data Model
NoteDto

- Id: SHA-256 hash of content
- Content: full note body
- CreationDate: DateTime
- LastModified: DateTime

## Markdown Output Format

## Note Title

Note body content with  
Markdown-style line breaks preserved.

_Created at: 06/02/2026 14:30_

## New Stack

Layer to technology mapping

- Frontend: Vue 3 + TypeScript + Vite + shadcn-vue + Tailwind (src/)
- Backend: Rust (Tauri) (src-tauri/)
- Database: SQLite (rusqlite or sqlx) (src-tauri/)
- Build: Vite (frontend), Cargo (Rust), Tauri CLI
- Bundler: Tauri bundler (MSI, NSIS, etc.)

Current State
The Tauri project is freshly scaffolded with the default Vue template. All application logic needs to be implemented.

Target Project Structure
xiaomi-note-exporter/
src/
App.vue
main.ts
router/index.ts
stores/
export.ts
sessions.ts
views/
converter-view.vue
export-view.vue
history-view.vue
viewer-view.vue
components/
converter/converter-form.vue
export/export-form.vue
export/export-progress.vue
history/session-list.vue
layout/app-header.vue
layout/app-sidebar.vue
viewer/markdown-editor.vue
viewer/markdown-preview.vue
lib/utils.ts
components/ui/ (shadcn-vue generated)
src-tauri/
src/
commands/
export.rs
sessions.rs
converter.rs
files.rs
scraper/
driver.rs
parser.rs
image.rs
db/
migrations.rs
models.rs

## GUI Design (Sidebar + Main Content)

Use shadcn-vue components with Tailwind. Support both light and dark themes (respect system preference via Tailwind dark: variants and a theme toggle in the sidebar).

Layout

- App-level sidebar is persistent in App.vue
- RouterView renders the active tab content
- Sidebar contains nav items: Export, History, Converter
- Bottom status badge shows export state and progress and navigates to Export on click

Tab 1 - Export (export-view.vue)
Export Form (export-form.vue)

- Domain input (default us.i.mi.com)
- Export directory picker (tauri-plugin-dialog)
- Split mode toggle + timestamp format input (default dd-MM-yyyy_HH-mm-ss)
- Export images toggle (default on)
- Start Export button (disabled if export running)
- Start Export opens a secondary Tauri webview for sign-in; scraping begins after auth

Export Progress (export-progress.vue)

- Progress bar (current / total)
- Percentage and elapsed time
- Live log list
- Cancel button
- On completion: success card + Open Folder button

Critical: Export must run in a background Rust task (tokio::spawn). UI remains responsive and users can switch tabs while running. Progress state lives in Pinia (export.ts) and is updated via Tauri events.

Tab 2 - History (history-view.vue)
Session List (session-list.vue)

- Table columns: Date/Time, Domain, Notes count, Images count, Split mode, Output path, Status
- Row actions: View, Open Folder, Delete
- Delete uses confirmation dialog; can optionally delete exported files

Tab 3 - Viewer (viewer-view.vue)

- File list sidebar for session files
- Main area with Tabs: Preview (Markdown rendering) and Edit (raw text editor)
- Toolbar: toggle preview/edit, Save, Export to JSON

Tab 4 - Converter (converter-view.vue)
Converter Form (converter-form.vue)

- Source selector: from session (dropdown) or file/folder picker
- Output path auto-generated or user-picked
- Convert button
- On completion: success message + Open File button

## Backend (Rust)

Responsibilities

1. Web scraping using Tauri webview or a Rust headless browser crate
2. Cookie and session management for authenticated image downloads
3. Image downloading with reqwest + session cookies
4. File I/O for Markdown and images
5. Date parsing with chrono (RelativeTimeParser, SimplifiedDateParser, multi-format parsing)
6. Markdown to JSON conversion (parse \*\*\*\* separators, SHA-256 IDs, serialize via serde_json)
7. SQLite sessions storage (single source of truth)
8. Tauri commands for frontend

Tauri Commands

- start_export(domain, output_dir, split, timestamp_format, export_images) -> session id
- cancel_export() -> void
- get_sessions(page, per_page) -> Vec<Session>
- get_session(id) -> Session
- delete_session(id, delete_files) -> void
- read_export_file(path) -> String
- write_export_file(path, content) -> void
- list_export_files(dir_path) -> Vec<FileEntry>
- convert_to_json(source_path, output_path) -> output path
- open_in_explorer(path) -> void

Communication Pattern
Use Tauri events for progress updates:

- export:progress { current, total, last_title }
- export:image { note_title, image_count }
- export:error { message }
- export:complete { session_id, total, elapsed }

## SQLite Database

DB location: Tauri app data dir (e.g. %APPDATA%/com.nogiszd.xiaomi-note-exporter/sessions.db)

Schema
CREATE TABLE IF NOT EXISTS sessions (
id TEXT PRIMARY KEY,
domain TEXT NOT NULL,
started_at TEXT NOT NULL,
completed_at TEXT,
status TEXT NOT NULL,
notes_count INTEGER DEFAULT 0,
images_count INTEGER DEFAULT 0,
split_mode INTEGER NOT NULL,
timestamp_fmt TEXT NOT NULL,
images_enabled INTEGER NOT NULL,
output_path TEXT NOT NULL,
error_message TEXT
);

Lifecycle

- On start: insert status running with notes_count 0
- During export: update notes_count and images_count
- On completion: status completed, completed_at set
- On cancel: status cancelled, completed_at set
- On error: status error, error_message set, completed_at set

## Mi Cloud DOM Selectors (fragile)

- Spinner: div[class*='spinner'] (wait for display: none)
- Create button: button[class*='btn-create'] (signals page ready)
- Note count: div[class*='note-count-select']
- Note list container: div[class*='note-list-items']
- Active/open note: div[class*='open']
- Note title: div[class*='origin-title'] > div
- Note content: div[class*='pm-container']
- Embedded images: .image-view img
- Sign-in detection: div[class*='ant-tabs'] (presence means not signed in)

## Markdown Formatting Rules

- Convert newlines to Markdown line breaks (two trailing spaces + newline)
- Separate notes with \*\*\*\* when combined in a single file
- Unsupported note types produce a placeholder entry

## Dependencies (Recommended)

Rust

- tauri
- serde, serde_json
- reqwest
- chrono
- sha2
- tokio
- rusqlite or sqlx
- uuid
- thiserror
- tauri-plugin-dialog
- tauri-plugin-fs (optional)

Frontend

- @tauri-apps/api
- @tauri-apps/plugin-dialog
- vue-router
- pinia
- shadcn-vue
- tailwindcss
- radix-vue
- class-variance-authority
- clsx or tailwind-merge
- lucide-vue-next
- markdown-it
- @vueuse/core (optional)
