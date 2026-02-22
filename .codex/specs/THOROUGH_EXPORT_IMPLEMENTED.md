## Thorough Export (Implemented)

### Summary

The Xiaomi Notes export now preserves rich structure from `div.pm-container` by parsing note HTML and converting it to Markdown in the Rust export pipeline.

This spec documents what is already implemented.

### Implemented Data Flow

1. Scraper collects both plain text and raw HTML from note body:
   - `content` from `noteContainer.innerText`
   - `contentHtml` from `noteContainer.innerHTML`
   - file: `src-tauri/src/services/scripts/scraper.js`
2. Payload includes HTML in `ScrapedNoteInput.content_html`:
   - file: `src-tauri/src/models.rs`
3. Export command passes HTML into markdown builder when available:
   - file: `src-tauri/src/commands/export.rs`
4. Markdown service parses HTML and renders structured Markdown:
   - file: `src-tauri/src/services/markdown.rs`

### Implemented HTML -> Markdown Mapping

#### Inline

- `<b>`, `<strong>` -> `**...**`
- `<i>`, `<em>` -> `*...*`
- `<u>` -> `<u>...</u>` (inline HTML preserved)
- `<del>`, `<s>`, `<strike>` -> `~~...~~`
- `<br>` -> newline in inline rendering

#### Block/Structure

- `<p class="pm-size-large">` -> `# ...`
- `<p class="pm-size-middle">` -> `## ...`
- `<p class="pm-size-h3">` -> `### ...`
- `<p>...</p>` -> paragraph line block
- `<blockquote>...</blockquote>` -> quote lines (`> ...`)
- `<div class="pm-order-list" ...>` -> ordered list item
- `<div class="pm-bullet-list" ...>` -> unordered list item
- `<div class="pm-checklist" ...>` -> checklist item
- unknown/malformed wrappers -> readable paragraph fallback

### Implemented List Rules

- `data-indentation` is parsed and clamped to `0..6` (fallback `0`)
- `data-start` for ordered lists:
  - numeric values are used
  - non-numeric values fall back to normal markdown sequence
- ordered list nesting width: `4` spaces per level
- unordered/checklist nesting width: `2` spaces per level
- list runs normalize base indentation to avoid unnecessary leading spaces

### Implemented Checklist Rules

- `data-checked="true"` -> `- [x] ...`
- any other value or missing attribute -> `- [ ] ...`

### Implemented Flow/Spacing Behavior

- Mixed content order is preserved across paragraphs/lists/quotes/checklists.
- Normal paragraph lines are emitted with markdown hard-break spacing (`two trailing spaces`) to preserve line breaks.
- A blank line is enforced after blockquotes so following lines do not continue the quote unintentionally.

### Robustness

- Parser traverses nested wrappers recursively.
- If a node is not recognized as a supported block, text is still extracted.
- If HTML parsing yields empty content, export falls back to existing plain-text linebreak conversion.

### Related Non-Parsing Export Updates

- Export elapsed time now starts when note total is reported (`report_export_total`), not at initial command start.

### Test Coverage (Current)

- Centralized under `src-tauri/src/tests/`.
- Parser-focused tests verify:
  - heading class mapping
  - list depth/numbering behavior
  - checklist checked/unchecked mapping
  - mixed flow ordering
  - paragraph behavior and quote termination
  - indentation normalization for nested list runs
- Main test file: `src-tauri/src/tests/markdown_tests.rs`

