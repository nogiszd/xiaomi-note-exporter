use std::{
    io::{Read, Write},
    net::TcpListener,
    thread,
    time::Duration,
};

use reqwest::blocking::Client;

use crate::services::update::{fetch_latest_release_version_from_api_url, normalize_version_tag};

fn spawn_one_shot_http_response(
    status_line: &str,
    body: &str,
    content_type: &str,
) -> (String, thread::JoinHandle<()>) {
    let listener = TcpListener::bind("127.0.0.1:0").expect("test listener should bind");
    let address = listener
        .local_addr()
        .expect("listener should expose local address");
    let status_line = status_line.to_string();
    let body = body.to_string();
    let content_type = content_type.to_string();

    let handle = thread::spawn(move || {
        let (mut stream, _) = listener.accept().expect("test request should connect");
        let mut buffer = [0_u8; 4096];
        let _ = stream.read(&mut buffer);

        let response = format!(
            "{status_line}\r\nContent-Type: {content_type}\r\nContent-Length: {}\r\nConnection: close\r\n\r\n{body}",
            body.len()
        );
        stream
            .write_all(response.as_bytes())
            .expect("response should be written");
        let _ = stream.flush();
    });

    (format!("http://{address}/latest"), handle)
}

fn test_http_client() -> Client {
    Client::builder()
        .timeout(Duration::from_secs(2))
        .build()
        .expect("test client should build")
}

#[test]
fn normalize_version_tag_trims_and_removes_v_prefix() {
    assert_eq!(normalize_version_tag(" v2.1.0 "), "2.1.0");
    assert_eq!(normalize_version_tag("V3.0.1"), "3.0.1");
    assert_eq!(normalize_version_tag("2.0.0"), "2.0.0");
}

#[test]
fn fetch_latest_release_version_from_api_url_returns_normalized_tag() {
    let (url, server_handle) = spawn_one_shot_http_response(
        "HTTP/1.1 200 OK",
        r#"{"tag_name":" v2.3.4 "}"#,
        "application/json",
    );

    let result = fetch_latest_release_version_from_api_url(&test_http_client(), &url)
        .expect("successful response should return parsed version");
    server_handle
        .join()
        .expect("test server thread should complete");

    assert_eq!(result, "2.3.4");
}

#[test]
fn fetch_latest_release_version_from_api_url_returns_http_error_status() {
    let (url, server_handle) =
        spawn_one_shot_http_response("HTTP/1.1 503 Service Unavailable", "{}", "application/json");

    let error = fetch_latest_release_version_from_api_url(&test_http_client(), &url)
        .expect_err("non-success status should fail");
    server_handle
        .join()
        .expect("test server thread should complete");

    assert!(error.to_string().contains("HTTP 503"));
}

#[test]
fn fetch_latest_release_version_from_api_url_returns_parse_error_for_invalid_payload() {
    let (url, server_handle) = spawn_one_shot_http_response(
        "HTTP/1.1 200 OK",
        r#"{"unexpected":"shape"}"#,
        "application/json",
    );

    let error = fetch_latest_release_version_from_api_url(&test_http_client(), &url)
        .expect_err("invalid response payload should fail");
    server_handle
        .join()
        .expect("test server thread should complete");

    assert!(error
        .to_string()
        .contains("Failed to parse latest release API"));
}

#[test]
fn fetch_latest_release_version_from_api_url_returns_error_for_empty_version_tag() {
    let (url, server_handle) = spawn_one_shot_http_response(
        "HTTP/1.1 200 OK",
        r#"{"tag_name":"   v   "}"#,
        "application/json",
    );

    let error = fetch_latest_release_version_from_api_url(&test_http_client(), &url)
        .expect_err("empty normalized tag should fail");
    server_handle
        .join()
        .expect("test server thread should complete");

    assert!(error.to_string().contains("empty tag"));
}
