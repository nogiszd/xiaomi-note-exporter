use std::path::PathBuf;

use tauri::State;

use crate::{
    db::sessions as sessions_db, error::CommandResult, models::Session, services::files,
    state::AppState,
};

#[tauri::command]
pub fn get_sessions(
    state: State<'_, AppState>,
    page: Option<u32>,
    per_page: Option<u32>,
) -> CommandResult<Vec<Session>> {
    sessions_db::fetch_sessions(&state.db_path, page.unwrap_or(1), per_page.unwrap_or(50))
        .map_err(Into::into)
}

#[tauri::command]
pub fn get_session(state: State<'_, AppState>, id: String) -> CommandResult<Option<Session>> {
    sessions_db::fetch_session_by_id(&state.db_path, &id).map_err(Into::into)
}

#[tauri::command]
pub fn delete_session(
    state: State<'_, AppState>,
    id: String,
    delete_files: bool,
) -> CommandResult<()> {
    let removed =
        sessions_db::delete_session_by_id(&state.db_path, &id).map_err(|e| e.to_string())?;
    if delete_files {
        if let Some(session) = removed {
            let path = PathBuf::from(session.output_path);
            files::remove_path(&path).map_err(|e| e.to_string())?;
        }
    }

    Ok(())
}
