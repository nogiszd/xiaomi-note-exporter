use serde::{Deserialize, Serialize};

#[derive(Debug, Clone, Serialize, Deserialize)]
#[serde(rename_all = "camelCase")]
pub struct Session {
    pub id: String,
    pub domain: String,
    pub started_at: String,
    pub completed_at: Option<String>,
    pub status: String,
    pub notes_count: u32,
    pub images_count: u32,
    pub split_mode: bool,
    pub timestamp_format: String,
    pub images_enabled: bool,
    pub output_path: String,
    pub images_dir_name: Option<String>,
    pub error_message: Option<String>,
}

#[derive(Debug, Clone, Serialize)]
#[serde(rename_all = "camelCase")]
pub struct FileEntry {
    pub name: String,
    pub path: String,
}

#[derive(Debug, Clone, Serialize)]
#[serde(rename_all = "camelCase")]
pub struct ExportProgressEvent {
    pub session_id: String,
    pub current: u32,
    pub total: u32,
    pub last_title: String,
    pub notes_count: u32,
    pub images_count: u32,
    pub log_line: String,
}

#[derive(Debug, Clone, Serialize)]
#[serde(rename_all = "camelCase")]
pub struct ExportCompleteEvent {
    pub session_id: String,
    pub total: u32,
    pub elapsed_ms: u64,
    pub output_path: String,
}

#[derive(Debug, Clone, Serialize)]
#[serde(rename_all = "camelCase")]
pub struct ExportErrorEvent {
    pub session_id: String,
    pub message: String,
}

#[derive(Debug, Clone, Deserialize)]
#[serde(rename_all = "camelCase")]
pub struct ScrapedImageInput {
    pub name: String,
    pub data_base64: String,
}

#[derive(Debug, Clone, Deserialize)]
#[serde(rename_all = "camelCase")]
pub struct ScrapedNoteInput {
    pub title: String,
    pub content: String,
    pub created_string: String,
    pub unsupported: bool,
    #[serde(default)]
    pub images: Vec<ScrapedImageInput>,
}

#[derive(Debug, Clone, Serialize)]
#[serde(rename_all = "camelCase")]
pub struct NoteDto {
    pub id: String,
    pub content: String,
    pub creation_date: String,
    pub last_modified: String,
}

#[derive(Debug, Clone, Serialize, Deserialize)]
#[serde(rename_all = "camelCase")]
pub struct AppSettings {
    pub default_export_dir: String,
    pub dark_mode: bool,
}
