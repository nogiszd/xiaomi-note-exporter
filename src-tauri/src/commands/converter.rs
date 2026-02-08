use crate::{error::CommandResult, services::converter};

#[tauri::command]
pub fn convert_to_json(source_path: String, output_path: String) -> CommandResult<String> {
    converter::convert_to_json(&source_path, &output_path).map_err(Into::into)
}
