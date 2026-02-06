use std::path::PathBuf;

use chrono::{Local, Utc};
use tauri::{AppHandle, Emitter, Manager, State};
use uuid::Uuid;

use crate::{
    db::sessions as sessions_db,
    error::{AppError, CommandResult},
    models::{
        ExportCompleteEvent, ExportErrorEvent, ExportProgressEvent, ScrapedNoteInput, Session,
    },
    services::{date_parser, files, markdown, scraper, settings},
    state::{ActiveExportState, AppState},
};

fn now_utc() -> String {
    Utc::now().to_rfc3339()
}

fn lock_error() -> AppError {
    AppError::Message("Internal state lock poisoned.".to_string())
}

fn take_active_export(state: &AppState) -> Result<ActiveExportState, AppError> {
    let mut guard = state.active_export.lock().map_err(|_| lock_error())?;
    guard.take().ok_or(AppError::ExportNotRunning)
}

fn emit_progress(app: &AppHandle, export: &ActiveExportState, last_title: &str, log_line: &str) {
    let total = export.total_notes.max(export.notes_count.max(1));
    let _ = app.emit(
        "export:progress",
        ExportProgressEvent {
            session_id: export.session_id.clone(),
            current: export.notes_count,
            total,
            last_title: last_title.to_string(),
            notes_count: export.notes_count,
            images_count: export.images_count,
            log_line: log_line.to_string(),
        },
    );
}

#[tauri::command]
pub fn start_export(
    app: AppHandle,
    state: State<'_, AppState>,
    domain: String,
    output_dir: String,
    split: bool,
    timestamp_format: String,
    export_images: bool,
) -> CommandResult<String> {
    {
        let guard = state
            .active_export
            .lock()
            .map_err(|_| lock_error().to_string())?;
        if guard.is_some() {
            return Err(AppError::ExportRunning.to_string());
        }
    }

    let resolved_output_dir = if output_dir.trim().is_empty() {
        let app_settings =
            settings::load_settings(&state.settings_path).map_err(|e| e.to_string())?;
        app_settings.default_export_dir
    } else {
        output_dir
    };

    let session_id = Uuid::new_v4().to_string();
    let stamp = Local::now().format("%d-%m-%Y_%H-%M-%S").to_string();
    let output_dir = PathBuf::from(resolved_output_dir);

    let output_root = if split {
        output_dir.join(format!("exported_notes_{stamp}"))
    } else {
        output_dir.join(format!("exported_notes_{stamp}.md"))
    };

    let images_dir = if split {
        output_root.join("images")
    } else {
        output_dir.join(format!("images_{stamp}"))
    };

    if split {
        std::fs::create_dir_all(&output_root).map_err(|e| e.to_string())?;
    } else {
        files::ensure_parent(&output_root).map_err(|e| e.to_string())?;
    }

    if export_images {
        std::fs::create_dir_all(&images_dir).map_err(|e| e.to_string())?;
    }

    let session = Session {
        id: session_id.clone(),
        domain: domain.clone(),
        started_at: now_utc(),
        completed_at: None,
        status: "running".to_string(),
        notes_count: 0,
        images_count: 0,
        split_mode: split,
        timestamp_format: timestamp_format.clone(),
        images_enabled: export_images,
        output_path: output_root.to_string_lossy().to_string(),
        error_message: None,
    };

    sessions_db::insert_session(&state.db_path, &session).map_err(|e| e.to_string())?;

    let active_export = ActiveExportState {
        session_id: session_id.clone(),
        split,
        chrono_timestamp_format: markdown::dotnet_to_chrono_format(&timestamp_format),
        export_images,
        output_root: output_root.clone(),
        images_dir,
        total_notes: 0,
        notes_count: 0,
        images_count: 0,
        started_at: std::time::Instant::now(),
        auth_window_label: scraper::auth_window_label(&session_id),
    };

    {
        let mut guard = state
            .active_export
            .lock()
            .map_err(|_| lock_error().to_string())?;
        *guard = Some(active_export);
    }

    if let Err(error) = scraper::create_auth_window(&app, &session_id, &domain, export_images) {
        let _ = sessions_db::set_session_outcome(
            &state.db_path,
            &session_id,
            "error",
            &now_utc(),
            0,
            0,
            Some(&error.to_string()),
        );
        if let Ok(mut guard) = state.active_export.lock() {
            *guard = None;
        }
        return Err(error.to_string());
    }

    let _ = app.emit(
        "export:progress",
        ExportProgressEvent {
            session_id: session_id.clone(),
            current: 0,
            total: 1,
            last_title: String::new(),
            notes_count: 0,
            images_count: 0,
            log_line: format!(
                "Opened Xiaomi sign-in window for domain {domain}. Complete login to start export."
            ),
        },
    );

    Ok(session_id)
}

#[tauri::command]
pub fn cancel_export(app: AppHandle, state: State<'_, AppState>) -> CommandResult<()> {
    let export = match take_active_export(&state) {
        Ok(export) => export,
        Err(AppError::ExportNotRunning) => return Ok(()),
        Err(error) => return Err(error.to_string()),
    };

    sessions_db::set_session_outcome(
        &state.db_path,
        &export.session_id,
        "cancelled",
        &now_utc(),
        export.notes_count,
        export.images_count,
        None,
    )
    .map_err(|e| e.to_string())?;

    if let Some(window) = app.get_webview_window(&export.auth_window_label) {
        let _ = window.close();
    }

    let _ = app.emit(
        "export:error",
        ExportErrorEvent {
            session_id: export.session_id,
            message: "Export cancelled by user.".to_string(),
        },
    );

    Ok(())
}

#[tauri::command]
pub fn report_export_total(
    app: AppHandle,
    state: State<'_, AppState>,
    session_id: String,
    total: u32,
) -> CommandResult<()> {
    let mut guard = state
        .active_export
        .lock()
        .map_err(|_| lock_error().to_string())?;
    let export = guard
        .as_mut()
        .ok_or_else(|| AppError::ExportNotRunning.to_string())?;
    if export.session_id != session_id {
        return Err(AppError::SessionMismatch.to_string());
    }

    export.total_notes = total.max(1);
    emit_progress(
        &app,
        export,
        "",
        &format!("Discovered {} notes.", export.total_notes),
    );
    Ok(())
}

#[tauri::command]
pub fn append_scraped_note(
    app: AppHandle,
    state: State<'_, AppState>,
    session_id: String,
    note: ScrapedNoteInput,
) -> CommandResult<()> {
    let mut guard = state
        .active_export
        .lock()
        .map_err(|_| lock_error().to_string())?;
    let export = guard
        .as_mut()
        .ok_or_else(|| AppError::ExportNotRunning.to_string())?;
    if export.session_id != session_id {
        return Err(AppError::SessionMismatch.to_string());
    }

    let created_at = date_parser::parse_created_date(&note.created_string);
    let note_index = export.notes_count + 1;

    let mut image_links = Vec::new();
    if export.export_images {
        let images_dir_name = export
            .images_dir
            .file_name()
            .and_then(|name| name.to_str())
            .unwrap_or("images")
            .to_string();

        for (index, image) in note.images.iter().enumerate() {
            if image.data_base64.trim().is_empty() {
                continue;
            }

            let source_name = if image.name.trim().is_empty() {
                format!(
                    "note_img_{}_{}_{}.png",
                    note_index,
                    index,
                    created_at.format(&export.chrono_timestamp_format)
                )
            } else {
                image.name.clone()
            };

            let mut image_name = markdown::sanitize_filename(&source_name);
            if !image_name.to_ascii_lowercase().ends_with(".png") {
                image_name.push_str(".png");
            }

            let image_path = export.images_dir.join(&image_name);
            files::save_base64_image(&image_path, &image.data_base64).map_err(|e| e.to_string())?;
            export.images_count += 1;

            let relative_path = if export.split {
                format!("images/{image_name}")
            } else {
                format!("{images_dir_name}/{image_name}")
            };
            image_links.push(format!("![image {}]({relative_path})", index + 1));
        }
    }

    let markdown_note = crate::services::markdown::build_note_markdown(
        &note.title,
        &note.content,
        &image_links,
        created_at,
        note.unsupported,
    );

    if export.split {
        let mut file_name = markdown::sanitize_filename(&format!(
            "note_{}_{:04}.md",
            created_at.format(&export.chrono_timestamp_format),
            note_index
        ));

        if !file_name.to_ascii_lowercase().ends_with(".md") {
            file_name.push_str(".md");
        }

        let file_path = export.output_root.join(file_name);
        std::fs::write(file_path, markdown_note).map_err(|e| e.to_string())?;
    } else {
        files::append_text(&export.output_root, &markdown_note).map_err(|e| e.to_string())?;
    }

    export.notes_count += 1;
    sessions_db::update_session_progress(
        &state.db_path,
        &export.session_id,
        export.notes_count,
        export.images_count,
    )
    .map_err(|e| e.to_string())?;

    let log_line = if note.unsupported {
        format!("Processed note {} (unsupported type).", export.notes_count)
    } else {
        format!("Processed note {}: {}", export.notes_count, note.title)
    };
    emit_progress(&app, export, &note.title, &log_line);

    Ok(())
}

#[tauri::command]
pub fn finish_scrape(
    app: AppHandle,
    state: State<'_, AppState>,
    session_id: String,
) -> CommandResult<()> {
    let export = {
        let mut guard = state
            .active_export
            .lock()
            .map_err(|_| lock_error().to_string())?;
        let taken = guard
            .take()
            .ok_or_else(|| AppError::ExportNotRunning.to_string())?;
        if taken.session_id != session_id {
            *guard = Some(taken);
            return Err(AppError::SessionMismatch.to_string());
        }
        taken
    };

    sessions_db::set_session_outcome(
        &state.db_path,
        &export.session_id,
        "completed",
        &now_utc(),
        export.notes_count,
        export.images_count,
        None,
    )
    .map_err(|e| e.to_string())?;

    if let Some(window) = app.get_webview_window(&export.auth_window_label) {
        let _ = window.close();
    }

    let _ = app.emit(
        "export:complete",
        ExportCompleteEvent {
            session_id: export.session_id,
            total: export.notes_count,
            elapsed_ms: export.started_at.elapsed().as_millis() as u64,
            output_path: export.output_root.to_string_lossy().to_string(),
        },
    );

    Ok(())
}

#[tauri::command]
pub fn fail_scrape(
    app: AppHandle,
    state: State<'_, AppState>,
    session_id: String,
    message: String,
) -> CommandResult<()> {
    let export = {
        let mut guard = state
            .active_export
            .lock()
            .map_err(|_| lock_error().to_string())?;
        let taken = guard
            .take()
            .ok_or_else(|| AppError::ExportNotRunning.to_string())?;
        if taken.session_id != session_id {
            *guard = Some(taken);
            return Err(AppError::SessionMismatch.to_string());
        }
        taken
    };

    sessions_db::set_session_outcome(
        &state.db_path,
        &export.session_id,
        "error",
        &now_utc(),
        export.notes_count,
        export.images_count,
        Some(&message),
    )
    .map_err(|e| e.to_string())?;

    let _ = app.emit(
        "export:error",
        ExportErrorEvent {
            session_id: export.session_id,
            message,
        },
    );

    if let Some(window) = app.get_webview_window(&export.auth_window_label) {
        let _ = window.close();
    }

    Ok(())
}
