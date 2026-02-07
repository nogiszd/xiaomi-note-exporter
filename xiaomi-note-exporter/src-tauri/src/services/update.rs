use std::time::Duration;

use reqwest::blocking::Client;
use serde::Deserialize;

use crate::error::{AppError, AppResult};

const LATEST_RELEASE_API_URL: &str =
    "https://api.github.com/repos/nogiszd/xiaomi-note-exporter/releases/latest";

#[derive(Debug, Deserialize)]
struct LatestReleaseApiResponse {
    tag_name: String,
}

fn normalize_version_tag(raw: &str) -> String {
    raw.trim()
        .trim_start_matches(|c: char| c == 'v' || c == 'V')
        .to_string()
}

pub fn fetch_latest_release_version() -> AppResult<String> {
    let client = Client::builder()
        .timeout(Duration::from_secs(20))
        .user_agent("xiaomi-note-exporter")
        .build()
        .map_err(|e| AppError::Message(format!("Failed to initialize HTTP client: {e}")))?;

    fetch_latest_release_version_from_api(&client)
}

fn fetch_latest_release_version_from_api(client: &Client) -> AppResult<String> {
    let response = client
        .get(LATEST_RELEASE_API_URL)
        .send()
        .map_err(|e| AppError::Message(format!("Failed to fetch latest release API: {e}")))?;

    if !response.status().is_success() {
        return Err(AppError::Message(format!(
            "Failed to fetch latest release API: HTTP {}",
            response.status()
        )));
    }

    let payload = response
        .json::<LatestReleaseApiResponse>()
        .map_err(|e| AppError::Message(format!("Failed to parse latest release API: {e}")))?;

    let normalized = normalize_version_tag(&payload.tag_name);
    if normalized.is_empty() {
        return Err(AppError::Message(
            "Latest release API returned an empty tag.".to_string(),
        ));
    }

    Ok(normalized)
}
