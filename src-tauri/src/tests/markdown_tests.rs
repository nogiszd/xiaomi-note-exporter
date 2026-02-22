use crate::services::markdown::to_markdown_from_html;

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
