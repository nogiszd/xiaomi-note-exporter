use std::path::Path;

use crate::{error::CommandResult, models::FileEntry, services::files};

#[tauri::command]
pub fn read_export_file(path: String) -> CommandResult<String> {
    files::read_export_file(&path).map_err(Into::into)
}

#[tauri::command]
pub fn write_export_file(path: String, content: String) -> CommandResult<()> {
    files::write_export_file(&path, &content).map_err(Into::into)
}

#[tauri::command]
pub fn list_export_files(dir_path: String) -> CommandResult<Vec<FileEntry>> {
    files::list_export_files(&dir_path).map_err(Into::into)
}

#[tauri::command]
pub fn open_in_explorer(path: String) -> CommandResult<()> {
    files::open_in_system_explorer(Path::new(&path)).map_err(Into::into)
}
