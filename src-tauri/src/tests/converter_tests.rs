use std::{
    fs,
    path::{Path, PathBuf},
    time::{SystemTime, UNIX_EPOCH},
};

use crate::services::converter::{
    convert_to_json, is_markdown_file, parse_notes_from_markdown, parse_single_note,
};

struct TestDir {
    path: PathBuf,
}

impl TestDir {
    fn new(name: &str) -> Self {
        let unique = SystemTime::now()
            .duration_since(UNIX_EPOCH)
            .expect("system time should be after unix epoch")
            .as_nanos();
        let path = std::env::temp_dir().join(format!(
            "xiaomi_note_exporter_converter_{name}_{}_{}",
            std::process::id(),
            unique
        ));
        fs::create_dir_all(&path).expect("temp test directory should be created");
        Self { path }
    }
}

impl Drop for TestDir {
    fn drop(&mut self) {
        let _ = fs::remove_dir_all(&self.path);
    }
}

fn note_section(title: &str, body: &str, created: &str) -> String {
    format!("****\n## {title}\n{body}\n*Created at: {created}*\n")
}

#[test]
fn markdown_extension_detection_is_case_insensitive() {
    assert!(is_markdown_file(Path::new("note.md")));
    assert!(is_markdown_file(Path::new("note.MD")));
    assert!(is_markdown_file(Path::new("note.markdown")));
    assert!(!is_markdown_file(Path::new("note.txt")));
}

#[test]
fn parse_single_note_extracts_body_and_created_date() {
    let raw = note_section("Title", "line 1\nline 2", "22/02/2026 10:30");

    let note = parse_single_note(&raw).expect("single note should parse");

    assert_eq!(note.content, "line 1\nline 2");
    assert!(!note.id.is_empty());
    assert_eq!(note.creation_date, note.last_modified);
}

#[test]
fn parse_single_note_fails_without_created_line() {
    let error =
        parse_single_note("## Title\nBody only\n").expect_err("created line should be required");

    assert!(error.to_string().contains("missing created date line"));
}

#[test]
fn parse_notes_from_markdown_handles_multiple_sections() {
    let content = format!(
        "{}\n{}",
        note_section("First", "alpha", "22/02/2026 10:30"),
        note_section("Second", "beta", "22/02/2026 10:31"),
    );

    let notes = parse_notes_from_markdown(&content).expect("multiple notes should parse");

    assert_eq!(notes.len(), 2);
    assert_eq!(notes[0].content, "alpha");
    assert_eq!(notes[1].content, "beta");
}

#[test]
fn convert_to_json_reads_only_markdown_files_from_directory() {
    let test_dir = TestDir::new("reads_only_markdown");
    let source = test_dir.path.join("source");
    let output = test_dir.path.join("output").join("notes.json");
    fs::create_dir_all(&source).expect("source directory should exist");

    fs::write(
        source.join("a.md"),
        note_section("Title", "body-md", "22/02/2026 10:30"),
    )
    .expect("markdown source should be written");
    fs::write(
        source.join("ignore.txt"),
        note_section("Ignored", "body-txt", "22/02/2026 10:31"),
    )
    .expect("non-markdown source should be written");

    let written_path = convert_to_json(
        source.to_string_lossy().as_ref(),
        output.to_string_lossy().as_ref(),
    )
    .expect("conversion should succeed");

    assert_eq!(PathBuf::from(written_path), output);

    let raw_json = fs::read_to_string(&output).expect("output json should be readable");
    let parsed: serde_json::Value =
        serde_json::from_str(&raw_json).expect("output should be valid json");
    let notes = parsed.as_array().expect("json root should be an array");

    assert_eq!(notes.len(), 1);
    assert_eq!(notes[0]["content"], "body-md");
}

#[test]
fn convert_to_json_errors_when_no_markdown_files_exist() {
    let test_dir = TestDir::new("no_markdown");
    let source = test_dir.path.join("source");
    let output = test_dir.path.join("output").join("notes.json");
    fs::create_dir_all(&source).expect("source directory should exist");

    fs::write(source.join("note.txt"), "not markdown").expect("source file should be written");

    let error = convert_to_json(
        source.to_string_lossy().as_ref(),
        output.to_string_lossy().as_ref(),
    )
    .expect_err("conversion should fail without markdown inputs");

    assert!(error.to_string().contains("No markdown notes were found"));
}
