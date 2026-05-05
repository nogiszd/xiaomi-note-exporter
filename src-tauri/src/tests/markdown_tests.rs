use chrono::{Local, TimeZone};

use crate::services::markdown::{
    build_note_markdown, dotnet_to_chrono_created_date_format, dotnet_to_chrono_format,
    sanitize_filename, to_markdown_from_html,
};

fn make_split_filename(formatted_date: &str, note_index: u32) -> String {
    let mut name = sanitize_filename(&format!("note_{}_{:04}.md", formatted_date, note_index));
    if !name.to_ascii_lowercase().ends_with(".md") {
        name.push_str(".md");
    }
    name
}

#[test]
fn dotnet_to_chrono_format_converts_underscore_format() {
    assert_eq!(
        dotnet_to_chrono_format("dd-MM-yyyy_HH-mm-ss"),
        "%d-%m-%Y_%H-%M-%S"
    );
}

#[test]
fn dotnet_to_chrono_format_converts_slash_and_colon_format() {
    assert_eq!(
        dotnet_to_chrono_format("yyyy/MM/dd HH:mm:ss"),
        "%Y/%m/%d %H:%M:%S"
    );
}

#[test]
fn dotnet_to_chrono_format_falls_back_on_empty_input() {
    assert_eq!(dotnet_to_chrono_format(""), "%d-%m-%Y_%H-%M-%S");
}

#[test]
fn dotnet_to_chrono_format_falls_back_on_unrecognized_input() {
    assert_eq!(dotnet_to_chrono_format("not-a-format"), "%d-%m-%Y_%H-%M-%S");
}

#[test]
fn dotnet_to_chrono_created_date_format_converts_default_format() {
    assert_eq!(
        dotnet_to_chrono_created_date_format("dd/MM/yyyy HH:mm"),
        "%d/%m/%Y %H:%M"
    );
}

#[test]
fn dotnet_to_chrono_created_date_format_converts_iso_like_format() {
    assert_eq!(
        dotnet_to_chrono_created_date_format("yyyy-MM-dd HH:mm:ss"),
        "%Y-%m-%d %H:%M:%S"
    );
}

#[test]
fn dotnet_to_chrono_created_date_format_falls_back_on_empty_input() {
    assert_eq!(dotnet_to_chrono_created_date_format(""), "%d/%m/%Y %H:%M");
}

#[test]
fn dotnet_to_chrono_created_date_format_falls_back_on_unrecognized_input() {
    assert_eq!(
        dotnet_to_chrono_created_date_format("not-a-format"),
        "%d/%m/%Y %H:%M"
    );
}

#[test]
fn dotnet_to_chrono_created_date_format_uses_distinct_fallback_from_filename_format() {
    assert_ne!(
        dotnet_to_chrono_created_date_format(""),
        dotnet_to_chrono_format("")
    );
}

#[test]
fn build_note_markdown_uses_default_created_date_format() {
    let created_at = Local
        .with_ymd_and_hms(2026, 5, 5, 13, 42, 0)
        .single()
        .expect("valid local datetime");
    let chrono_format = dotnet_to_chrono_created_date_format("dd/MM/yyyy HH:mm");

    let output = build_note_markdown(
        "Title",
        "body",
        None,
        &[],
        created_at,
        &chrono_format,
        false,
    );

    assert!(
        output.ends_with("*Created at: 05/05/2026 13:42*\n"),
        "output did not end with default-format footer: {output:?}"
    );
}

#[test]
fn build_note_markdown_honors_custom_created_date_format() {
    let created_at = Local
        .with_ymd_and_hms(2026, 5, 5, 13, 42, 0)
        .single()
        .expect("valid local datetime");
    let chrono_format = dotnet_to_chrono_created_date_format("yyyy-MM-dd HH:mm:ss");

    let output = build_note_markdown(
        "Title",
        "body",
        None,
        &[],
        created_at,
        &chrono_format,
        false,
    );

    assert!(
        output.ends_with("*Created at: 2026-05-05 13:42:00*\n"),
        "output did not end with custom-format footer: {output:?}"
    );
}

#[test]
fn build_note_markdown_falls_back_to_default_when_format_is_empty() {
    let created_at = Local
        .with_ymd_and_hms(2026, 5, 5, 13, 42, 0)
        .single()
        .expect("valid local datetime");
    let chrono_format = dotnet_to_chrono_created_date_format("");

    let output = build_note_markdown(
        "Title",
        "body",
        None,
        &[],
        created_at,
        &chrono_format,
        false,
    );

    assert!(
        output.ends_with("*Created at: 05/05/2026 13:42*\n"),
        "output did not fall back to default format: {output:?}"
    );
}

#[test]
fn build_note_markdown_falls_back_to_default_when_format_is_unrecognized() {
    let created_at = Local
        .with_ymd_and_hms(2026, 5, 5, 13, 42, 0)
        .single()
        .expect("valid local datetime");
    let chrono_format = dotnet_to_chrono_created_date_format("not-a-format");

    let output = build_note_markdown(
        "Title",
        "body",
        None,
        &[],
        created_at,
        &chrono_format,
        false,
    );

    assert!(
        output.ends_with("*Created at: 05/05/2026 13:42*\n"),
        "output did not fall back to default format: {output:?}"
    );
}

#[test]
fn build_note_markdown_default_and_custom_formats_differ() {
    let created_at = Local
        .with_ymd_and_hms(2026, 5, 5, 13, 42, 0)
        .single()
        .expect("valid local datetime");
    let default_format = dotnet_to_chrono_created_date_format("dd/MM/yyyy HH:mm");
    let custom_format = dotnet_to_chrono_created_date_format("yyyy-MM-dd");

    let default_output =
        build_note_markdown("Title", "body", None, &[], created_at, &default_format, false);
    let custom_output =
        build_note_markdown("Title", "body", None, &[], created_at, &custom_format, false);

    assert_ne!(default_output, custom_output);
}

#[test]
fn split_filename_safe_format_passes_through_unchanged() {
    assert_eq!(
        make_split_filename("01-04-2026_14-30-00", 1),
        "note_01-04-2026_14-30-00_0001.md"
    );
}

#[test]
fn split_filename_sanitizes_slashes_in_date() {
    assert_eq!(
        make_split_filename("01/04/2026_14-30-00", 2),
        "note_01_04_2026_14-30-00_0002.md"
    );
}

#[test]
fn split_filename_sanitizes_colons_in_date() {
    assert_eq!(
        make_split_filename("2026-04-01 14:30:00", 3),
        "note_2026-04-01 14_30_00_0003.md"
    );
}

#[test]
fn split_filename_zero_pads_note_index_to_four_digits() {
    assert_eq!(
        make_split_filename("01-04-2026_14-30-00", 42),
        "note_01-04-2026_14-30-00_0042.md"
    );
}

#[test]
fn split_filename_always_ends_with_md_extension() {
    let name = make_split_filename("01-04-2026_14-30-00", 1);
    assert!(name.ends_with(".md"));
}

#[test]
fn maps_heading_classes() {
    let html = r#"
            <p class="pm-size-large">Main</p>
            <p class="pm-size-middle">Section</p>
            <p class="pm-size-h3">Detail</p>
        "#;

    let output = to_markdown_from_html(html);

    assert_eq!(output, "# Main\n## Section\n### Detail");
}

#[test]
fn renders_nested_list_depth_and_order() {
    let html = r#"
            <div class="pm-order-list" data-indentation="0" data-start="3">Root</div>
            <div class="pm-order-list" data-indentation="1" data-start="a">Nested fallback</div>
            <div class="pm-bullet-list" data-indentation="2"><b>Deep</b> node</div>
        "#;

    let output = to_markdown_from_html(html);

    assert_eq!(
        output,
        "3. Root\n    1. Nested fallback\n    - **Deep** node"
    );
}

#[test]
fn renders_checklist_checked_state() {
    let html = r#"
            <div class="pm-checklist" data-checked="true">Done</div>
            <div class="pm-checklist" data-checked="false">Todo</div>
            <div class="pm-checklist">Unset</div>
        "#;

    let output = to_markdown_from_html(html);

    assert_eq!(output, "- [x] Done\n- [ ] Todo\n- [ ] Unset");
}

#[test]
fn preserves_mixed_content_flow() {
    let html = r#"
            <p>Intro <i>line</i></p>
            <div class="pm-bullet-list" data-indentation="0">Item <del>old</del></div>
            <blockquote><u>Quoted</u><br/>line</blockquote>
            <div class="pm-checklist" data-indentation="1" data-checked="true">Task</div>
        "#;

    let output = to_markdown_from_html(html);

    assert_eq!(
        output,
        "Intro *line*  \n\n- Item ~~old~~\n\n> <u>Quoted</u>\n> line\n\n- [x] Task"
    );
}

#[test]
fn preserves_plain_text_around_inline_markdown_inside_p() {
    let html = r#"<p>before <b>bold</b> after <i>italics</i> tail</p>"#;

    let output = to_markdown_from_html(html);

    assert_eq!(output, "before **bold** after *italics* tail");
}

#[test]
fn keeps_p_blocks_as_separate_lines() {
    let html = r#"
            <div>
              <p>Line 1</p>
              <p><b>Line</b> 2</p>
              <p>Line 3</p>
            </div>
        "#;

    let output = to_markdown_from_html(html);

    assert_eq!(output, "Line 1  \n**Line** 2  \nLine 3");
}

#[test]
fn keeps_paragraphs_when_lists_exist_in_same_wrapper() {
    let html = r#"
            <div>
              <p>Before <b>item</b></p>
              <div class="pm-bullet-list" data-indentation="1">Nested bullet</div>
              <div class="pm-checklist" data-indentation="1" data-checked="true">Nested task</div>
              <p>After</p>
            </div>
        "#;

    let output = to_markdown_from_html(html);

    assert_eq!(
        output,
        "Before **item**  \n\n- Nested bullet\n- [x] Nested task\n\nAfter"
    );
}

#[test]
fn normalizes_list_base_indentation_to_avoid_leading_space_noise() {
    let html = r#"
            <div class="pm-bullet-list" data-indentation="2">Top</div>
            <div class="pm-bullet-list" data-indentation="3">Nested</div>
            <div class="pm-checklist" data-indentation="3" data-checked="true">Task</div>
        "#;

    let output = to_markdown_from_html(html);

    assert_eq!(output, "- Top\n  - Nested\n  - [x] Task");
}

#[test]
fn adds_blank_line_after_blockquote_before_following_text() {
    let html = r#"
            <blockquote>Quoted</blockquote>
            <p>After quote</p>
        "#;

    let output = to_markdown_from_html(html);

    assert_eq!(output, "> Quoted\n\nAfter quote");
}
