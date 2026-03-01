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

pub(crate) fn normalize_version_tag(raw: &str) -> String {
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
    fetch_latest_release_version_from_api_url(client, LATEST_RELEASE_API_URL)
}

pub(crate) fn fetch_latest_release_version_from_api_url(
    client: &Client,
    api_url: &str,
) -> AppResult<String> {
    let response = client
        .get(api_url)
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
