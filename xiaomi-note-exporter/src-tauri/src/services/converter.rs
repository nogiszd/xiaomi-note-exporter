use std::{fs, path::Path};

use sha2::{Digest, Sha256};
use walkdir::WalkDir;

use crate::{
    error::{AppError, AppResult},
    models::NoteDto,
    services::date_parser::parse_markdown_created_line,
};

fn sha256_hex(input: &str) -> String {
    let mut hasher = Sha256::new();
    hasher.update(input.as_bytes());
    hasher
        .finalize()
        .iter()
        .map(|byte| format!("{byte:02x}"))
        .collect::<String>()
}

fn is_markdown_file(path: &Path) -> bool {
    path.extension()
        .and_then(|ext| ext.to_str())
        .map(|ext| {
            let lowered = ext.to_ascii_lowercase();
            lowered == "md" || lowered == "markdown"
        })
        .unwrap_or(false)
}

fn parse_single_note(raw: &str) -> AppResult<NoteDto> {
    let mut lines = raw
        .replace("\r\n", "\n")
        .split('\n')
        .map(|line| line.trim_end().to_string())
        .collect::<Vec<_>>();

    let created_line_index = lines
        .iter()
        .rposition(|line| line.trim_start().starts_with("*Created at:"))
        .ok_or_else(|| {
            AppError::Message("Invalid note format: missing created date line.".to_string())
        })?;

    let created_line = lines[created_line_index].clone();
    let created_at = parse_markdown_created_line(&created_line)?;

    lines.remove(created_line_index);

    while lines
        .first()
        .is_some_and(|line| line.trim().is_empty() || line.trim() == "****")
    {
        lines.remove(0);
    }

    while lines.last().is_some_and(|line| line.trim().is_empty()) {
        lines.pop();
    }

    if lines.first().is_some_and(|line| line.starts_with("## ")) {
        lines.remove(0);
    }

    let content = lines.join("\n").trim().to_string();

    Ok(NoteDto {
        id: sha256_hex(&content),
        content,
        creation_date: created_at.clone(),
        last_modified: created_at,
    })
}

fn parse_notes_from_markdown(content: &str) -> AppResult<Vec<NoteDto>> {
    let sections = if content.contains("****") {
        content
            .split("****")
            .filter_map(|section| {
                let trimmed = section.trim();
                if trimmed.is_empty() {
                    None
                } else {
                    Some(trimmed.to_string())
                }
            })
            .collect::<Vec<_>>()
    } else {
        vec![content.trim().to_string()]
    };

    let mut notes = Vec::new();
    for section in sections {
        notes.push(parse_single_note(&section)?);
    }

    Ok(notes)
}

pub fn convert_to_json(source_path: &str, output_path: &str) -> AppResult<String> {
    let source = std::path::PathBuf::from(source_path);
    if !source.exists() {
        return Err(AppError::Message("Source path does not exist.".to_string()));
    }

    let mut notes = Vec::new();
    if source.is_dir() {
        for entry in WalkDir::new(&source)
            .min_depth(1)
            .into_iter()
            .filter_map(Result::ok)
            .filter(|entry| entry.file_type().is_file() && is_markdown_file(entry.path()))
        {
            let file_content = fs::read_to_string(entry.path())?;
            let parsed = parse_notes_from_markdown(&file_content)?;
            notes.extend(parsed);
        }
    } else if source.is_file() {
        let file_content = fs::read_to_string(&source)?;
        notes = parse_notes_from_markdown(&file_content)?;
    }

    if notes.is_empty() {
        return Err(AppError::Message(
            "No markdown notes were found to convert.".to_string(),
        ));
    }

    let output = std::path::PathBuf::from(output_path);
    if let Some(parent) = output.parent() {
        fs::create_dir_all(parent)?;
    }

    let json = serde_json::to_string_pretty(&notes)?;
    fs::write(&output, json)?;
    Ok(output.to_string_lossy().to_string())
}
