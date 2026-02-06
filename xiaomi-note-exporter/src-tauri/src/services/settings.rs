use std::{
    fs,
    path::{Path, PathBuf},
};

use crate::{
    error::{AppError, AppResult},
    models::AppSettings,
};

const DEFAULT_EXPORT_DIR_NAME: &str = "Xiaomi Note Exporter";

fn normalize_export_dir(dir: &str) -> String {
    let trimmed = dir.trim();
    if trimmed.is_empty() {
        String::new()
    } else {
        trimmed.to_string()
    }
}

fn default_export_dir(documents_dir: &Path) -> PathBuf {
    documents_dir.join(DEFAULT_EXPORT_DIR_NAME)
}

pub fn bootstrap_settings(settings_path: &Path, documents_dir: &Path) -> AppResult<AppSettings> {
    let settings = if settings_path.exists() {
        load_settings(settings_path)?
    } else {
        AppSettings {
            default_export_dir: default_export_dir(documents_dir)
                .to_string_lossy()
                .to_string(),
            dark_mode: false,
        }
    };

    let resolved_default_dir = {
        let normalized = normalize_export_dir(&settings.default_export_dir);
        if normalized.is_empty() {
            default_export_dir(documents_dir)
        } else {
            PathBuf::from(normalized)
        }
    };

    fs::create_dir_all(&resolved_default_dir)?;

    let normalized_settings = AppSettings {
        default_export_dir: resolved_default_dir.to_string_lossy().to_string(),
        dark_mode: settings.dark_mode,
    };

    save_settings(settings_path, &normalized_settings)?;
    Ok(normalized_settings)
}

pub fn load_settings(settings_path: &Path) -> AppResult<AppSettings> {
    if !settings_path.exists() {
        return Err(AppError::Message(
            "Settings file does not exist.".to_string(),
        ));
    }

    let raw = fs::read_to_string(settings_path)?;
    let settings: AppSettings = serde_json::from_str(&raw)?;
    Ok(settings)
}

pub fn save_settings(settings_path: &Path, settings: &AppSettings) -> AppResult<()> {
    let export_dir = normalize_export_dir(&settings.default_export_dir);
    if export_dir.is_empty() {
        return Err(AppError::Message(
            "Default export directory cannot be empty.".to_string(),
        ));
    }

    let export_path = PathBuf::from(export_dir);
    fs::create_dir_all(&export_path)?;

    if let Some(parent) = settings_path.parent() {
        fs::create_dir_all(parent)?;
    }

    let normalized = AppSettings {
        default_export_dir: export_path.to_string_lossy().to_string(),
        dark_mode: settings.dark_mode,
    };

    let serialized = serde_json::to_string_pretty(&normalized)?;
    fs::write(settings_path, serialized)?;

    Ok(())
}
