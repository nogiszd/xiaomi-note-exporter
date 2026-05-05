use chrono::{DateTime, Local};
use scraper::node::Node;
use scraper::{ElementRef, Html, Selector};

const MAX_LIST_DEPTH: usize = 6;
const ORDERED_LIST_INDENT_SPACES: usize = 4;
const UNORDERED_LIST_INDENT_SPACES: usize = 2;

#[derive(Debug, Clone, PartialEq, Eq)]
enum BlockKind {
    Blank,
    Paragraph,
    Heading(u8),
    Quote,
    OrderedList,
    BulletList,
    Checklist(bool),
}

#[derive(Debug, Clone, PartialEq, Eq)]
struct ParsedBlock {
    kind: BlockKind,
    text: String,
    indentation: usize,
    order_start: Option<usize>,
}

impl ParsedBlock {
    fn blank() -> Self {
        Self {
            kind: BlockKind::Blank,
            text: String::new(),
            indentation: 0,
            order_start: None,
        }
    }

    fn paragraph(text: String) -> Self {
        Self {
            kind: BlockKind::Paragraph,
            text,
            indentation: 0,
            order_start: None,
        }
    }

    fn heading(level: u8, text: String) -> Self {
        Self {
            kind: BlockKind::Heading(level),
            text,
            indentation: 0,
            order_start: None,
        }
    }

    fn quote(text: String) -> Self {
        Self {
            kind: BlockKind::Quote,
            text,
            indentation: 0,
            order_start: None,
        }
    }

    fn ordered(text: String, indentation: usize, order_start: Option<usize>) -> Self {
        Self {
            kind: BlockKind::OrderedList,
            text,
            indentation,
            order_start,
        }
    }

    fn bullet(text: String, indentation: usize) -> Self {
        Self {
            kind: BlockKind::BulletList,
            text,
            indentation,
            order_start: None,
        }
    }

    fn checklist(text: String, indentation: usize, checked: bool) -> Self {
        Self {
            kind: BlockKind::Checklist(checked),
            text,
            indentation,
            order_start: None,
        }
    }
}

fn dotnet_to_chrono_with_fallback(input: &str, fallback: &str) -> String {
    let mut output = input.to_string();
    let replacements = [
        ("yyyy", "%Y"),
        ("MM", "%m"),
        ("dd", "%d"),
        ("HH", "%H"),
        ("mm", "%M"),
        ("ss", "%S"),
    ];
    for (dotnet, chrono) in replacements {
        output = output.replace(dotnet, chrono);
    }
    if output.contains('%') {
        output
    } else {
        fallback.to_string()
    }
}

pub fn dotnet_to_chrono_format(input: &str) -> String {
    dotnet_to_chrono_with_fallback(input, "%d-%m-%Y_%H-%M-%S")
}

pub fn dotnet_to_chrono_created_date_format(input: &str) -> String {
    dotnet_to_chrono_with_fallback(input, "%d/%m/%Y %H:%M")
}

pub fn sanitize_filename(input: &str) -> String {
    let mut output = String::with_capacity(input.len());
    for character in input.chars() {
        let sanitized = match character {
            '<' | '>' | ':' | '"' | '/' | '\\' | '|' | '?' | '*' => '_',
            c if c.is_control() => '_',
            c => c,
        };
        output.push(sanitized);
    }

    let trimmed = output.trim_matches(|c: char| c.is_whitespace() || c == '.');
    if trimmed.is_empty() {
        "note".to_string()
    } else {
        trimmed.to_string()
    }
}

fn to_markdown_linebreaks(content: &str) -> String {
    content
        .lines()
        .map(|line| format!("{line}  "))
        .collect::<Vec<_>>()
        .join("\n")
}

fn has_class(element: &ElementRef<'_>, class_name: &str) -> bool {
    element
        .value()
        .attr("class")
        .map(|value| {
            value
                .split_whitespace()
                .any(|token| token == class_name || token.contains(class_name))
        })
        .unwrap_or(false)
}

fn parse_indentation(element: &ElementRef<'_>) -> usize {
    element
        .value()
        .attr("data-indentation")
        .and_then(|value| value.trim().parse::<isize>().ok())
        .unwrap_or(0)
        .clamp(0, MAX_LIST_DEPTH as isize) as usize
}

fn parse_order_start(element: &ElementRef<'_>) -> Option<usize> {
    element
        .value()
        .attr("data-start")
        .and_then(|value| value.trim().parse::<usize>().ok())
        .filter(|value| *value > 0)
}

fn is_checked(element: &ElementRef<'_>) -> bool {
    element
        .value()
        .attr("data-checked")
        .map(|value| value.eq_ignore_ascii_case("true"))
        .unwrap_or(false)
}

fn normalize_multiline_text(input: &str) -> String {
    let normalized = input
        .replace('\u{00a0}', " ")
        .replace("\r\n", "\n")
        .replace('\r', "\n");
    let mut lines = normalized
        .split('\n')
        .map(|line| line.trim().to_string())
        .collect::<Vec<_>>();

    while lines.first().is_some_and(|line| line.is_empty()) {
        lines.remove(0);
    }
    while lines.last().is_some_and(|line| line.is_empty()) {
        lines.pop();
    }

    if let Some(last_line) = lines.last_mut() {
        *last_line = last_line.trim_end().to_string();
    }

    lines.join("\n")
}

fn wrap_inline(marker: &str, value: String) -> String {
    if value.trim().is_empty() {
        String::new()
    } else {
        format!("{marker}{value}{marker}")
    }
}

fn render_inline_children(element: &ElementRef<'_>) -> String {
    let mut output = String::new();

    for child in element.children() {
        match child.value() {
            Node::Text(text) => output.push_str(text),
            Node::Element(_) => {
                if let Some(child_element) = ElementRef::wrap(child) {
                    output.push_str(&render_inline_element(&child_element));
                }
            }
            _ => {}
        }
    }

    output
}

fn render_inline_element(element: &ElementRef<'_>) -> String {
    match element.value().name() {
        "br" => "\n".to_string(),
        "b" | "strong" => wrap_inline("**", render_inline_children(element)),
        "i" | "em" => wrap_inline("*", render_inline_children(element)),
        "u" => {
            let text = render_inline_children(element);
            if text.trim().is_empty() {
                String::new()
            } else {
                format!("<u>{text}</u>")
            }
        }
        "del" | "s" | "strike" => wrap_inline("~~", render_inline_children(element)),
        _ => render_inline_children(element),
    }
}

fn parse_heading_level(element: &ElementRef<'_>) -> Option<u8> {
    if element.value().name() != "p" {
        return None;
    }

    if has_class(element, "pm-size-large") {
        Some(1)
    } else if has_class(element, "pm-size-middle") {
        Some(2)
    } else if has_class(element, "pm-size-h3") {
        Some(3)
    } else {
        None
    }
}

fn is_block_element(element: &ElementRef<'_>) -> bool {
    let tag = element.value().name();
    tag == "br"
        || tag == "p"
        || tag == "blockquote"
        || has_class(element, "pm-order-list")
        || has_class(element, "pm-bullet-list")
        || has_class(element, "pm-checklist")
}

fn has_descendant_block_elements(element: &ElementRef<'_>) -> bool {
    for child in element.children() {
        if let Some(child_element) = ElementRef::wrap(child) {
            if is_block_element(&child_element) || has_descendant_block_elements(&child_element) {
                return true;
            }
        }
    }
    false
}

fn flush_inline_buffer_as_paragraph(inline_buffer: &mut String, blocks: &mut Vec<ParsedBlock>) {
    let text = normalize_multiline_text(inline_buffer);
    inline_buffer.clear();
    if !text.is_empty() {
        blocks.push(ParsedBlock::paragraph(text));
    }
}

fn append_known_block_from_element(
    element: &ElementRef<'_>,
    blocks: &mut Vec<ParsedBlock>,
) -> bool {
    if element.value().name() == "br" {
        blocks.push(ParsedBlock::blank());
        return true;
    }

    if element.value().name() == "p" {
        let text = normalize_multiline_text(&render_inline_children(element));
        if let Some(level) = parse_heading_level(element) {
            blocks.push(ParsedBlock::heading(level, text));
        } else if text.is_empty() {
            blocks.push(ParsedBlock::blank());
        } else {
            blocks.push(ParsedBlock::paragraph(text));
        }
        return true;
    }

    if element.value().name() == "blockquote" {
        let text = normalize_multiline_text(&render_inline_children(element));
        blocks.push(ParsedBlock::quote(text));
        return true;
    }

    if has_class(element, "pm-order-list") {
        let text = normalize_multiline_text(&render_inline_children(element));
        blocks.push(ParsedBlock::ordered(
            text,
            parse_indentation(element),
            parse_order_start(element),
        ));
        return true;
    }

    if has_class(element, "pm-bullet-list") {
        let text = normalize_multiline_text(&render_inline_children(element));
        blocks.push(ParsedBlock::bullet(text, parse_indentation(element)));
        return true;
    }

    if has_class(element, "pm-checklist") {
        let text = normalize_multiline_text(&render_inline_children(element));
        blocks.push(ParsedBlock::checklist(
            text,
            parse_indentation(element),
            is_checked(element),
        ));
        return true;
    }

    false
}

fn append_blocks_from_element(element: &ElementRef<'_>, blocks: &mut Vec<ParsedBlock>) {
    if append_known_block_from_element(element, blocks) {
        return;
    }

    let mut inline_buffer = String::new();
    for child in element.children() {
        match child.value() {
            Node::Text(text) => inline_buffer.push_str(text),
            Node::Element(_) => {
                if let Some(child_element) = ElementRef::wrap(child) {
                    if is_block_element(&child_element) {
                        flush_inline_buffer_as_paragraph(&mut inline_buffer, blocks);
                        append_blocks_from_element(&child_element, blocks);
                    } else if has_descendant_block_elements(&child_element) {
                        flush_inline_buffer_as_paragraph(&mut inline_buffer, blocks);
                        append_blocks_from_element(&child_element, blocks);
                    } else {
                        inline_buffer.push_str(&render_inline_element(&child_element));
                    }
                }
            }
            _ => {}
        }
    }

    flush_inline_buffer_as_paragraph(&mut inline_buffer, blocks);
}

fn parse_blocks_from_html(content_html: &str) -> Vec<ParsedBlock> {
    let wrapped = format!("<div data-note-root=\"1\">{content_html}</div>");
    let document = Html::parse_document(&wrapped);
    let selector = Selector::parse("div[data-note-root='1']").ok();
    let Some(root_selector) = selector else {
        return Vec::new();
    };
    let Some(root) = document.select(&root_selector).next() else {
        return Vec::new();
    };

    let mut blocks = Vec::new();
    append_blocks_from_element(&root, &mut blocks);
    blocks
}

fn push_list_lines(
    lines: &mut Vec<String>,
    indentation: usize,
    marker: &str,
    text: &str,
    indent_spaces: usize,
) {
    let prefix = " ".repeat(indentation * indent_spaces);
    let continuation_padding = " ".repeat(marker.len() + 1);
    let text_lines = normalize_multiline_text(text)
        .split('\n')
        .map(|line| line.trim_end().to_string())
        .collect::<Vec<_>>();

    if text_lines.is_empty() || (text_lines.len() == 1 && text_lines[0].is_empty()) {
        lines.push(format!("{prefix}{marker}"));
        return;
    }

    lines.push(format!("{prefix}{marker} {}", text_lines[0]));
    for line in text_lines.iter().skip(1) {
        if line.is_empty() {
            lines.push(format!("{prefix}{continuation_padding}"));
        } else {
            lines.push(format!("{prefix}{continuation_padding}{line}"));
        }
    }
}

fn push_paragraph_lines(lines: &mut Vec<String>, text: &str) {
    let paragraph_lines = normalize_multiline_text(text)
        .split('\n')
        .map(|line| line.trim_end().to_string())
        .collect::<Vec<_>>();

    for line in paragraph_lines {
        if line.is_empty() {
            lines.push(String::new());
        } else {
            lines.push(format!("{line}  "));
        }
    }
}

fn render_blocks_to_markdown(blocks: &[ParsedBlock]) -> String {
    if blocks.is_empty() {
        return String::new();
    }

    let mut lines = Vec::<String>::new();
    let mut previous_is_list = false;
    let mut previous_was_ordered = false;
    let mut previous_was_quote = false;
    let mut list_base_indentation: Option<usize> = None;
    let mut ordered_counters = [1usize; MAX_LIST_DEPTH + 1];

    for block in blocks {
        if previous_was_quote
            && !matches!(block.kind, BlockKind::Blank)
            && !lines.last().is_some_and(|line| line.is_empty())
        {
            lines.push(String::new());
        }

        match block.kind {
            BlockKind::Blank => {
                if !lines.last().is_some_and(|line| line.is_empty()) {
                    lines.push(String::new());
                }
                previous_is_list = false;
                previous_was_ordered = false;
                previous_was_quote = false;
                list_base_indentation = None;
                ordered_counters.fill(1);
            }
            BlockKind::Heading(level) => {
                if previous_is_list && !lines.last().is_some_and(|line| line.is_empty()) {
                    lines.push(String::new());
                }
                let text = block.text.trim();
                if text.is_empty() {
                    lines.push("#".repeat(level as usize));
                } else {
                    lines.push(format!("{} {text}", "#".repeat(level as usize)));
                }
                previous_is_list = false;
                previous_was_ordered = false;
                previous_was_quote = false;
                list_base_indentation = None;
                ordered_counters.fill(1);
            }
            BlockKind::Paragraph => {
                if previous_is_list && !lines.last().is_some_and(|line| line.is_empty()) {
                    lines.push(String::new());
                }
                if !block.text.trim().is_empty() {
                    push_paragraph_lines(&mut lines, &block.text);
                }
                previous_is_list = false;
                previous_was_ordered = false;
                previous_was_quote = false;
                list_base_indentation = None;
                ordered_counters.fill(1);
            }
            BlockKind::Quote => {
                if previous_is_list && !lines.last().is_some_and(|line| line.is_empty()) {
                    lines.push(String::new());
                }
                let normalized_quote = normalize_multiline_text(&block.text);
                let quote_lines = normalized_quote
                    .split('\n')
                    .map(str::trim)
                    .collect::<Vec<_>>();
                if quote_lines.is_empty() {
                    lines.push(">".to_string());
                } else {
                    for quote_line in quote_lines {
                        if quote_line.is_empty() {
                            lines.push(">".to_string());
                        } else {
                            lines.push(format!("> {quote_line}"));
                        }
                    }
                }
                previous_is_list = false;
                previous_was_ordered = false;
                previous_was_quote = true;
                list_base_indentation = None;
                ordered_counters.fill(1);
            }
            BlockKind::OrderedList => {
                if !previous_is_list
                    && !lines.is_empty()
                    && !lines.last().is_some_and(|line| line.is_empty())
                {
                    lines.push(String::new());
                }

                if !previous_is_list {
                    list_base_indentation = Some(block.indentation);
                }
                let base = list_base_indentation.unwrap_or(0);
                let effective_indentation = block.indentation.saturating_sub(base);

                for depth in (effective_indentation + 1)..=MAX_LIST_DEPTH {
                    ordered_counters[depth] = 1;
                }

                let value = if let Some(explicit_start) = block.order_start {
                    explicit_start
                } else {
                    ordered_counters[effective_indentation]
                };
                ordered_counters[effective_indentation] = value.saturating_add(1);

                push_list_lines(
                    &mut lines,
                    effective_indentation,
                    &format!("{value}."),
                    &block.text,
                    ORDERED_LIST_INDENT_SPACES,
                );

                previous_is_list = true;
                previous_was_ordered = true;
                previous_was_quote = false;
            }
            BlockKind::BulletList => {
                if !previous_is_list
                    && !lines.is_empty()
                    && !lines.last().is_some_and(|line| line.is_empty())
                {
                    lines.push(String::new());
                }

                if !previous_is_list {
                    list_base_indentation = Some(block.indentation);
                }
                let base = list_base_indentation.unwrap_or(0);
                let effective_indentation = block.indentation.saturating_sub(base);

                let was_ordered = previous_was_ordered;
                push_list_lines(
                    &mut lines,
                    effective_indentation,
                    "-",
                    &block.text,
                    UNORDERED_LIST_INDENT_SPACES,
                );

                previous_is_list = true;
                previous_was_ordered = false;
                previous_was_quote = false;
                if was_ordered {
                    ordered_counters.fill(1);
                }
            }
            BlockKind::Checklist(checked) => {
                if !previous_is_list
                    && !lines.is_empty()
                    && !lines.last().is_some_and(|line| line.is_empty())
                {
                    lines.push(String::new());
                }

                if !previous_is_list {
                    list_base_indentation = Some(block.indentation);
                }
                let base = list_base_indentation.unwrap_or(0);
                let effective_indentation = block.indentation.saturating_sub(base);

                let was_ordered = previous_was_ordered;
                let marker = if checked { "- [x]" } else { "- [ ]" };
                push_list_lines(
                    &mut lines,
                    effective_indentation,
                    marker,
                    &block.text,
                    UNORDERED_LIST_INDENT_SPACES,
                );

                previous_is_list = true;
                previous_was_ordered = false;
                previous_was_quote = false;
                if was_ordered {
                    ordered_counters.fill(1);
                }
            }
        }
    }

    while lines.last().is_some_and(|line| line.is_empty()) {
        lines.pop();
    }

    if let Some(last_line) = lines.last_mut() {
        *last_line = last_line.trim_end().to_string();
    }

    lines.join("\n")
}

pub(crate) fn to_markdown_from_html(content_html: &str) -> String {
    if content_html.trim().is_empty() {
        return String::new();
    }

    let blocks = parse_blocks_from_html(content_html);
    render_blocks_to_markdown(&blocks)
}

pub fn build_note_markdown(
    title: &str,
    content: &str,
    content_html: Option<&str>,
    image_links: &[String],
    created_at: DateTime<Local>,
    created_date_format: &str,
    unsupported: bool,
) -> String {
    let safe_title = if title.trim().is_empty() {
        "Untitled Note"
    } else {
        title.trim()
    };

    let body = if unsupported {
        "**Unsupported note type (Mind-map or Sound note)**".to_string()
    } else {
        let rich = content_html.map(to_markdown_from_html).unwrap_or_default();
        if rich.trim().is_empty() {
            to_markdown_linebreaks(content)
        } else {
            rich
        }
    };

    let mut markdown = String::new();
    markdown.push_str("****\n");
    markdown.push_str(&format!("## Title: {safe_title}\n"));
    markdown.push_str(&body);
    markdown.push_str("\n\n");

    if !image_links.is_empty() {
        for link in image_links {
            markdown.push_str(link);
            markdown.push('\n');
        }
        markdown.push('\n');
    }

    markdown.push_str(&format!(
        "*Created at: {}*\n",
        created_at.format(created_date_format)
    ));

    markdown
}
