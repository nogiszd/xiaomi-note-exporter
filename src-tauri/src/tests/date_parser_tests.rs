use chrono::{DateTime, Datelike, Local, Timelike, Utc};

use crate::services::date_parser::{
    parse_created_date, parse_created_line_to_rfc3339, parse_markdown_created_line,
};

#[test]
fn parses_rfc3339_created_line_without_changing_instant() {
    let input = "2024-01-02T03:04:05+02:00";

    let output = parse_created_line_to_rfc3339(input).expect("rfc3339 input should parse");
    let expected = DateTime::parse_from_rfc3339(input)
        .expect("valid input rfc3339")
        .with_timezone(&Utc);
    let actual = DateTime::parse_from_rfc3339(&output)
        .expect("valid output rfc3339")
        .with_timezone(&Utc);

    assert_eq!(actual, expected);
}

#[test]
fn parses_markdown_created_line_markers() {
    let output = parse_markdown_created_line("*Created at: 22/02/2026 11:45*")
        .expect("markdown created line should parse");
    let parsed_local = DateTime::parse_from_rfc3339(&output)
        .expect("valid output rfc3339")
        .with_timezone(&Local);

    assert_eq!(parsed_local.year(), 2026);
    assert_eq!(parsed_local.month(), 2);
    assert_eq!(parsed_local.day(), 22);
    assert_eq!(parsed_local.hour(), 11);
    assert_eq!(parsed_local.minute(), 45);
}

#[test]
fn rejects_markdown_created_line_without_value() {
    let error =
        parse_markdown_created_line("*Created at:*").expect_err("empty value should be rejected");

    assert!(error.to_string().contains("missing created date value"));
}

#[test]
fn parses_simplified_month_day_and_time() {
    let parsed = parse_created_date("12/31 23:59");

    assert_eq!(parsed.month(), 12);
    assert_eq!(parsed.day(), 31);
    assert_eq!(parsed.hour(), 23);
    assert_eq!(parsed.minute(), 59);
}

#[test]
fn parses_relative_time_hours_ago() {
    let parsed = parse_created_date("2 hours ago");
    let seconds = (Local::now() - parsed).num_seconds();

    assert!(
        (7080..=7320).contains(&seconds),
        "expected ~7200s delta, got {seconds}"
    );
}
