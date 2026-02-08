use chrono::{DateTime, Datelike, Local, NaiveDateTime, TimeZone, Utc};

use crate::error::{AppError, AppResult};

fn local_from_parts(
    year: i32,
    month: u32,
    day: u32,
    hour: u32,
    minute: u32,
) -> Option<DateTime<Local>> {
    Local
        .with_ymd_and_hms(year, month, day, hour, minute, 0)
        .single()
        .or_else(|| {
            Local
                .with_ymd_and_hms(year, month, day, hour, minute, 0)
                .earliest()
        })
}

fn parse_relative_time(input: &str) -> Option<DateTime<Local>> {
    let normalized = input.trim().to_lowercase();
    let parts = normalized.split_whitespace().collect::<Vec<_>>();
    if parts.len() != 3 || parts[2] != "ago" {
        return None;
    }

    let value = parts[0].parse::<i64>().ok()?;
    let unit = parts[1];
    let seconds = match unit {
        "second" | "seconds" => value,
        "minute" | "minutes" => value * 60,
        "hour" | "hours" => value * 3600,
        "day" | "days" => value * 86400,
        "week" | "weeks" => value * 7 * 86400,
        "month" | "months" => value * 30 * 86400,
        "year" | "years" => value * 365 * 86400,
        _ => return None,
    };

    Some(Local::now() - chrono::Duration::seconds(seconds))
}

fn parse_simplified_md_hm(input: &str) -> Option<DateTime<Local>> {
    let chunks = input.trim().split(' ').collect::<Vec<_>>();
    if chunks.len() != 2 {
        return None;
    }

    let date_parts = chunks[0].split('/').collect::<Vec<_>>();
    let time_parts = chunks[1].split(':').collect::<Vec<_>>();
    if date_parts.len() != 2 || time_parts.len() != 2 {
        return None;
    }

    let month = date_parts[0].parse::<u32>().ok()?;
    let day = date_parts[1].parse::<u32>().ok()?;
    let hour = time_parts[0].parse::<u32>().ok()?;
    let minute = time_parts[1].parse::<u32>().ok()?;

    local_from_parts(Local::now().year(), month, day, hour, minute)
}

pub fn parse_created_date(input: &str) -> DateTime<Local> {
    let normalized = input.trim();
    if normalized.is_empty() {
        return Local::now();
    }

    let lower = normalized.to_lowercase();
    if lower.contains("now") {
        return Local::now();
    }

    if lower.contains("yesterday") {
        return Local::now() - chrono::Duration::days(1);
    }

    if lower.ends_with("ago") {
        if let Some(parsed) = parse_relative_time(&lower) {
            return parsed;
        }
    }

    if let Some(parsed) = parse_simplified_md_hm(normalized) {
        return parsed;
    }

    let formats = [
        "%d/%m/%Y %H:%M",
        "%m/%d/%Y %H:%M",
        "%Y/%m/%d %H:%M",
        "%d/%m/%Y %-H:%M",
        "%m/%d/%Y %-H:%M",
        "%m/%d/%Y %I:%M %p",
        "%-m/%-d/%Y %-I:%M %p",
    ];

    for format in formats {
        if let Ok(naive) = NaiveDateTime::parse_from_str(normalized, format) {
            if let Some(local) = Local.from_local_datetime(&naive).single() {
                return local;
            }
            if let Some(local) = Local.from_local_datetime(&naive).earliest() {
                return local;
            }
        }
    }

    if let Ok(parsed_rfc) = DateTime::parse_from_rfc3339(normalized) {
        return parsed_rfc.with_timezone(&Local);
    }

    Local::now()
}

pub fn parse_created_line_to_rfc3339(input: &str) -> AppResult<String> {
    let cleaned = input.trim();

    if let Ok(parsed_rfc) = DateTime::parse_from_rfc3339(cleaned) {
        return Ok(parsed_rfc.with_timezone(&Utc).to_rfc3339());
    }

    let parsed = parse_created_date(cleaned);
    Ok(parsed.with_timezone(&Utc).to_rfc3339())
}

pub fn parse_markdown_created_line(input: &str) -> AppResult<String> {
    let cleaned = input
        .trim()
        .trim_start_matches("*Created at:")
        .trim_end_matches('*')
        .trim();

    if cleaned.is_empty() {
        return Err(AppError::Message(
            "Invalid note format: missing created date value.".to_string(),
        ));
    }

    parse_created_line_to_rfc3339(cleaned)
}
