use std::{
    fs,
    io::{self, Write},
    path::{Path, PathBuf},
    process::Command,
};

use base64::{engine::general_purpose::STANDARD, Engine};
use walkdir::WalkDir;

use crate::{
    error::{AppError, AppResult},
    models::FileEntry,
};

pub fn ensure_parent(path: &Path) -> AppResult<()> {
    if let Some(parent) = path.parent() {
        fs::create_dir_all(parent)?;
    }
    Ok(())
}

pub fn append_text(path: &Path, content: &str) -> AppResult<()> {
    ensure_parent(path)?;
    let mut file = fs::OpenOptions::new()
        .create(true)
        .append(true)
        .open(path)?;
    file.write_all(content.as_bytes())?;
    Ok(())
}

pub fn read_export_file(path: &str) -> AppResult<String> {
    Ok(fs::read_to_string(path)?)
}

pub fn write_export_file(path: &str, content: &str) -> AppResult<()> {
    let output_path = PathBuf::from(path);
    ensure_parent(&output_path)?;
    fs::write(output_path, content)?;
    Ok(())
}

pub fn list_export_files(dir_path: &str) -> AppResult<Vec<FileEntry>> {
    let root = PathBuf::from(dir_path);
    if root.is_file() {
        let name = root
            .file_name()
            .and_then(|value| value.to_str())
            .unwrap_or_default()
            .to_string();

        return Ok(vec![FileEntry {
            name,
            path: root.to_string_lossy().to_string(),
        }]);
    }

    if !root.is_dir() {
        return Err(AppError::Message(
            "Provided path is not a file or directory.".to_string(),
        ));
    }

    let mut files = WalkDir::new(&root)
        .min_depth(1)
        .into_iter()
        .filter_map(Result::ok)
        .filter(|entry| entry.file_type().is_file())
        .map(|entry| FileEntry {
            name: entry.file_name().to_string_lossy().to_string(),
            path: entry.path().to_string_lossy().to_string(),
        })
        .collect::<Vec<_>>();

    files.sort_by(|left, right| left.path.cmp(&right.path));
    Ok(files)
}

pub fn save_base64_image(path: &Path, data_base64: &str) -> AppResult<()> {
    let decoded = STANDARD.decode(data_base64)?;
    ensure_parent(path)?;
    fs::write(path, decoded)?;
    Ok(())
}

pub fn remove_path(path: &Path) -> AppResult<()> {
    if path.is_file() {
        if let Err(error) = fs::remove_file(path) {
            if error.kind() != io::ErrorKind::NotFound {
                return Err(error.into());
            }
        }
    } else if path.is_dir() {
        if let Err(error) = fs::remove_dir_all(path) {
            if error.kind() != io::ErrorKind::NotFound {
                return Err(error.into());
            }
        }
    }

    Ok(())
}

pub fn remove_export_artifacts(
    output_path: &Path,
    split_mode: bool,
    images_dir_name: Option<&str>,
) -> AppResult<()> {
    remove_path(output_path)?;

    if let Some(images_dir_name) = images_dir_name {
        let trimmed = images_dir_name.trim();
        if !trimmed.is_empty() {
            let images_path = if split_mode {
                output_path.join(trimmed)
            } else if let Some(parent) = output_path.parent() {
                parent.join(trimmed)
            } else {
                PathBuf::from(trimmed)
            };

            remove_path(&images_path)?;
        }
    }

    Ok(())
}

pub fn open_in_system_explorer(path: &Path) -> AppResult<()> {
    #[cfg(target_os = "windows")]
    {
        let mut command = Command::new("explorer");
        if path.is_file() {
            command.arg("/select,").arg(path);
        } else {
            command.arg(path);
        }
        command.spawn()?;
        return Ok(());
    }

    #[cfg(target_os = "macos")]
    {
        Command::new("open").arg(path).spawn()?;
        return Ok(());
    }

    #[cfg(target_os = "linux")]
    {
        Command::new("xdg-open").arg(path).spawn()?;
        return Ok(());
    }

    #[allow(unreachable_code)]
    Err(AppError::Message(
        "open_in_explorer is not supported on this platform.".to_string(),
    ))
}
