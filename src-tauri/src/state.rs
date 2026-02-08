use std::{
    path::PathBuf,
    sync::{Arc, Mutex},
    time::Instant,
};

#[derive(Clone)]
pub struct AppState {
    pub db_path: PathBuf,
    pub settings_path: PathBuf,
    pub active_export: Arc<Mutex<Option<ActiveExportState>>>,
}

impl AppState {
    pub fn new(db_path: PathBuf, settings_path: PathBuf) -> Self {
        Self {
            db_path,
            settings_path,
            active_export: Arc::new(Mutex::new(None)),
        }
    }
}

#[derive(Debug, Clone)]
pub struct ActiveExportState {
    pub session_id: String,
    pub split: bool,
    pub chrono_timestamp_format: String,
    pub export_images: bool,
    pub output_root: PathBuf,
    pub images_dir: PathBuf,
    pub total_notes: u32,
    pub notes_count: u32,
    pub images_count: u32,
    pub started_at: Instant,
    pub auth_window_label: String,
}
