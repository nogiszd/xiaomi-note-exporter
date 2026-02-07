mod commands;
mod db;
mod error;
mod models;
mod services;
mod state;

use std::{fs, io};

use tauri::Manager;

use crate::{db::sessions::init_db, services::settings, state::AppState};

#[cfg_attr(mobile, tauri::mobile_entry_point)]
pub fn run() {
    #[cfg(target_os = "windows")]
    {
        std::env::set_var(
            "WEBVIEW2_ADDITIONAL_BROWSER_ARGUMENTS",
            "--disable-features=CalculateNativeWinOcclusion",
        );
    }

    tauri::Builder::default()
        .plugin(tauri_plugin_dialog::init())
        .plugin(tauri_plugin_opener::init())
        .setup(|app| {
            let app_data_dir = app
                .path()
                .app_data_dir()
                .map_err(|error| io::Error::other(error.to_string()))?;
            fs::create_dir_all(&app_data_dir)?;

            let db_path = app_data_dir.join("sessions.db");
            let settings_path = app_data_dir.join("settings.json");
            let documents_dir = app
                .path()
                .document_dir()
                .unwrap_or_else(|_| app_data_dir.clone());
            init_db(&db_path).map_err(|error| io::Error::other(error.to_string()))?;
            settings::bootstrap_settings(&settings_path, &documents_dir)
                .map_err(|error| io::Error::other(error.to_string()))?;
            app.manage(AppState::new(db_path, settings_path));
            Ok(())
        })
        .invoke_handler(tauri::generate_handler![
            commands::export::start_export,
            commands::export::cancel_export,
            commands::export::report_export_total,
            commands::export::append_scraped_note,
            commands::export::finish_scrape,
            commands::export::fail_scrape,
            commands::sessions::get_sessions,
            commands::sessions::get_session,
            commands::sessions::delete_session,
            commands::files::read_export_file,
            commands::files::write_export_file,
            commands::files::list_export_files,
            commands::files::open_in_explorer,
            commands::converter::convert_to_json,
            commands::settings::get_app_settings,
            commands::settings::update_app_settings,
            commands::settings::check_latest_release_version
        ])
        .run(tauri::generate_context!())
        .expect("error while running tauri application");
}
