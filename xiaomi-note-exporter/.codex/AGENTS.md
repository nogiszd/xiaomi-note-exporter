## Mission

Build the Tauri + Vue rewrite of Xiaomi Note Exporter. Export notes from Mi Cloud to Markdown (with images) and support JSON conversion.

## Always Do

- Use shadcn-vue components via the MCP server configured in config.toml (do not hand-roll UI primitives).
- Prefer Composition API with TypeScript.
- Keep exports running in background tasks; UI must remain responsive.
- Record every export session in SQLite.

## Key Constraints

- No Selenium or external ChromeDriver.
- Sidebar is persistent in App.vue.
- Use Tauri events for live progress updates.
- Keep all behavior consistent with the legacy CLI (see PLANS.md).

## Where To Look

- Detailed architecture, UI, schema, and commands: PLANS.md.
- Legacy reference: ../xiaomiNoteExporter/ (C#).

---

## Development

```bash
# Install frontend dependencies
npm install

# Run in development mode (starts Vite dev server + Tauri window)
npm run tauri dev

# Build for production
npm run tauri build
```

---

## Coding Conventions

- **Rust**: Follow standard Rust idioms. Use `snake_case` for functions/variables, `PascalCase` for types. Handle errors with `Result<T, E>` — avoid `unwrap()` in production paths. Use `thiserror` for custom error types.
- **TypeScript/Vue**: Use Composition API (`<script setup lang="ts">`). Prefer `ref`/`reactive` for state. Use TypeScript strict mode.
- **File naming**: `kebab-case` for Vue components and TS files; `snake_case` for Rust files.
- **Comments**: Document public Rust functions with `///` doc comments. Use JSDoc or inline comments for non-obvious TypeScript logic.

---

## Important Implementation Notes

1. **No Selenium** — The rewrite must not depend on an external browser or ChromeDriver. Tauri's webview (or a Rust headless-browser crate) should handle all web interaction.
2. **Cross-platform potential** — While the original was Windows-only, Tauri enables cross-platform builds. Keep platform-specific code minimal.
3. **Mi Cloud DOM selectors** — The scraper relies on specific CSS classes/XPaths from the Mi Cloud web app. These are fragile and may change. Key selectors from the original:
   - Spinner: `div[class*='spinner']` (wait for `display: none`)
   - Create button: `button[class*='btn-create']` (signals page is ready)
   - Note count: `div[class*='note-count-select']`
   - Note list container: `div[class*='note-list-items']`
   - Active/open note: `div[class*='open']`
   - Note title: `div[class*='origin-title'] > div`
   - Note content: `div[class*='pm-container']`
   - Embedded images: `.image-view img`
   - Sign-in detection: `div[class*='ant-tabs']` (presence means not signed in)
4. **Image authentication** — Downloading images requires passing the Mi Cloud session cookies. After sign-in, cookies must be extracted from the webview and attached to HTTP download requests.
5. **Unsupported note types** — Mind-map and Sound notes cannot be exported as text. The app should detect these (notes without a content container) and output a placeholder entry.
6. **Markdown formatting** — Newlines in note content should be converted to Markdown line breaks (two trailing spaces + newline). Notes are separated by `****` when combined in a single file.
7. **Background export is critical** — The export must run in a `tokio::spawn` task so the GUI remains fully responsive. The user must be able to navigate to History or Converter tabs while an export is in progress. Progress updates flow via Tauri events, not polling.
8. **SQLite as single source of truth** — Every export session (successful, cancelled, or errored) is recorded in the SQLite DB. The History tab reads exclusively from the DB. The output path stored in the DB allows the Viewer and Converter to locate files.
9. **shadcn-vue theming** — Use shadcn-vue's default "New York" style. Support both light and dark themes (respect system preference via Tailwind's `dark:` variant and a theme toggle in the sidebar).
10. **Sidebar always visible** — The sidebar is a persistent layout element in `App.vue`, not part of any individual view. Use Vue Router's `<RouterView>` for the main content area.
