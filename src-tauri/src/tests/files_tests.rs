use std::{
    fs,
    path::PathBuf,
    time::{SystemTime, UNIX_EPOCH},
};

use crate::services::files::{
    append_text, list_export_files, remove_export_artifacts, save_base64_image, write_export_file,
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
            "xiaomi_note_exporter_files_{name}_{}_{}",
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

#[test]
fn append_text_creates_parent_and_appends_content() {
    let test_dir = TestDir::new("append_text");
    let file = test_dir.path.join("nested").join("note.md");

    append_text(&file, "first").expect("first append should succeed");
    append_text(&file, " second").expect("second append should succeed");

    let content = fs::read_to_string(file).expect("output file should be readable");
    assert_eq!(content, "first second");
}

#[test]
fn write_export_file_creates_parent_directories() {
    let test_dir = TestDir::new("write_export_file");
    let file = test_dir.path.join("a").join("b").join("note.md");

    write_export_file(file.to_string_lossy().as_ref(), "hello")
        .expect("write_export_file should create parent directories");

    let content = fs::read_to_string(file).expect("written file should be readable");
    assert_eq!(content, "hello");
}

#[test]
fn list_export_files_returns_single_entry_for_file_path() {
    let test_dir = TestDir::new("list_single");
    let file = test_dir.path.join("single.md");
    fs::write(&file, "x").expect("test file should be written");

    let files = list_export_files(file.to_string_lossy().as_ref())
        .expect("listing a file path should succeed");

    assert_eq!(files.len(), 1);
    assert_eq!(files[0].name, "single.md");
}

#[test]
fn list_export_files_returns_sorted_entries_for_directory() {
    let test_dir = TestDir::new("list_directory");
    let b = test_dir.path.join("b.md");
    let a = test_dir.path.join("a.md");
    fs::write(&b, "b").expect("b file should be written");
    fs::write(&a, "a").expect("a file should be written");

    let files = list_export_files(test_dir.path.to_string_lossy().as_ref())
        .expect("listing a directory should succeed");

    assert_eq!(files.len(), 2);
    assert!(files[0].path < files[1].path);
}

#[test]
fn save_base64_image_decodes_and_writes_bytes() {
    let test_dir = TestDir::new("save_base64");
    let image_path = test_dir.path.join("images").join("one.bin");

    save_base64_image(&image_path, "aGVsbG8=").expect("base64 data should decode");

    let bytes = fs::read(image_path).expect("image bytes should be readable");
    assert_eq!(bytes, b"hello");
}

#[test]
fn remove_export_artifacts_removes_non_split_output_and_images_dir() {
    let test_dir = TestDir::new("remove_non_split");
    let output_file = test_dir.path.join("exported.md");
    let images_dir = test_dir.path.join("images_1");
    fs::write(&output_file, "note").expect("output file should be written");
    fs::create_dir_all(&images_dir).expect("images directory should be created");
    fs::write(images_dir.join("img.png"), "img").expect("image should be written");

    remove_export_artifacts(&output_file, false, Some("images_1"))
        .expect("non-split artifact removal should succeed");

    assert!(!output_file.exists());
    assert!(!images_dir.exists());
}

#[test]
fn remove_export_artifacts_removes_split_output_directory() {
    let test_dir = TestDir::new("remove_split");
    let output_dir = test_dir.path.join("exported_notes");
    let images_dir = output_dir.join("images");
    fs::create_dir_all(&images_dir).expect("split output directories should be created");
    fs::write(images_dir.join("img.png"), "img").expect("image should be written");

    remove_export_artifacts(&output_dir, true, Some("images"))
        .expect("split artifact removal should succeed");

    assert!(!output_dir.exists());
}
