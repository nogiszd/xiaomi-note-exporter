use tauri::State;

use crate::{
    error::CommandResult,
    models::AppSettings,
    services::{settings, update},
    state::AppState,
};

#[tauri::command]
pub fn get_app_settings(state: State<'_, AppState>) -> CommandResult<AppSettings> {
    settings::load_settings(&state.settings_path).map_err(Into::into)
}

#[tauri::command]
pub fn update_app_settings(
    state: State<'_, AppState>,
    settings_payload: AppSettings,
) -> CommandResult<AppSettings> {
    settings::save_settings(&state.settings_path, &settings_payload).map_err(|e| e.to_string())?;
    settings::load_settings(&state.settings_path).map_err(Into::into)
}

#[tauri::command]
pub fn check_latest_release_version() -> CommandResult<String> {
    update::fetch_latest_release_version().map_err(Into::into)
}
