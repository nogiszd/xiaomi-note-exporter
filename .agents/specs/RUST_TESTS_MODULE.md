## Rust Tests Module (`src-tauri/src/tests`)

### Purpose

Centralize Rust unit tests in one place so test code stays organized and service modules stay focused on runtime logic.

### Scope

This module covers service-level tests for:

- `services::date_parser`
- `services::converter`
- `services::files`
- `services::markdown`
- `services::scraper`
- `services::update`

### Layout

- Test root: `src-tauri/src/tests/mod.rs`
- Test files:
  - `src-tauri/src/tests/date_parser_tests.rs`
  - `src-tauri/src/tests/converter_tests.rs`
  - `src-tauri/src/tests/files_tests.rs`
  - `src-tauri/src/tests/markdown_tests.rs`
  - `src-tauri/src/tests/scraper_tests.rs`
  - `src-tauri/src/tests/update_tests.rs`

### Wiring

Tests are registered once in crate root:

- `src-tauri/src/lib.rs` includes `#[cfg(test)] mod tests;`

The test root file imports each test file as a module:

- `mod date_parser_tests;`
- `mod converter_tests;`
- `mod files_tests;`
- `mod markdown_tests;`
- `mod scraper_tests;`
- `mod update_tests;`

### Access Pattern

Test files import functions through crate paths, for example:

- `use crate::services::date_parser::...`
- `use crate::services::converter::...`

Some helper functions used by tests are exposed as `pub(crate)` (not public API).

### Why This Design

- Avoids per-service `mod tests` path-resolution issues.
- Keeps tests discoverable in one folder.
- Makes it easy to scale test coverage by module.

### Commands

- Run all Rust tests:
  - `cargo test --manifest-path src-tauri/Cargo.toml`
- Format Rust files:
  - `cargo fmt --manifest-path src-tauri/Cargo.toml`

### Adding Tests for a New Service Module

1. Add a new file in `src-tauri/src/tests`, e.g. `settings_tests.rs`.
2. Register it in `src-tauri/src/tests/mod.rs`.
3. Import targets using `crate::services::<module>::...`.
4. If needed, expose internals as `pub(crate)` in the service file.

