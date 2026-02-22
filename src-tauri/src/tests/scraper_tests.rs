use crate::services::scraper::{
    auth_window_label, build_notes_url, build_scrape_script, AUTH_WINDOW_LABEL_PREFIX,
};

#[test]
fn auth_window_label_prefixes_session_id() {
    let session_id = "session-123";
    let label = auth_window_label(session_id);

    assert_eq!(label, format!("{AUTH_WINDOW_LABEL_PREFIX}-{session_id}"));
}

#[test]
fn build_notes_url_uses_expected_micloud_notes_path() {
    let url = build_notes_url("us.i.mi.com").expect("valid domain should produce URL");

    assert_eq!(url.as_str(), "https://us.i.mi.com/note/h5/?_locale=en-US");
}

#[test]
fn build_notes_url_rejects_invalid_domain() {
    let error = build_notes_url("bad domain").expect_err("invalid domain should fail URL parse");

    assert!(
        error
            .to_string()
            .contains("invalid international domain name")
            || error.to_string().contains("invalid domain character")
    );
}

#[test]
fn build_scrape_script_replaces_runtime_placeholders() {
    let script = build_scrape_script("abc123", true);

    assert!(script.contains(r#"const sessionId = "abc123";"#));
    assert!(script.contains("const exportImages = true;"));
    assert!(!script.contains("__SESSION_ID__"));
    assert!(!script.contains("__EXPORT_IMAGES__"));
}
