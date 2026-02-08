use chrono::{DateTime, Local};

pub fn dotnet_to_chrono_format(input: &str) -> String {
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
        "%d-%m-%Y_%H-%M-%S".to_string()
    }
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

pub fn build_note_markdown(
    title: &str,
    content: &str,
    image_links: &[String],
    created_at: DateTime<Local>,
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
        to_markdown_linebreaks(content)
    };

    let mut markdown = String::new();
    markdown.push_str("****\n");
    markdown.push_str(&format!("## {safe_title}\n"));
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
        created_at.format("%d/%m/%Y %H:%M")
    ));

    markdown
}
