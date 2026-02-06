use std::path::Path;

use rusqlite::{params, Connection, OptionalExtension, Row};

use crate::{error::AppResult, models::Session};

fn bool_to_i64(value: bool) -> i64 {
    if value {
        1
    } else {
        0
    }
}

fn map_session_row(row: &Row<'_>) -> Result<Session, rusqlite::Error> {
    Ok(Session {
        id: row.get("id")?,
        domain: row.get("domain")?,
        started_at: row.get("started_at")?,
        completed_at: row.get("completed_at")?,
        status: row.get("status")?,
        notes_count: row.get::<_, i64>("notes_count")? as u32,
        images_count: row.get::<_, i64>("images_count")? as u32,
        split_mode: row.get::<_, i64>("split_mode")? != 0,
        timestamp_format: row.get("timestamp_fmt")?,
        images_enabled: row.get::<_, i64>("images_enabled")? != 0,
        output_path: row.get("output_path")?,
        images_dir_name: row.get("images_dir_name")?,
        error_message: row.get("error_message")?,
    })
}

pub fn init_db(db_path: &Path) -> AppResult<()> {
    let conn = Connection::open(db_path)?;
    conn.execute_batch(
        "CREATE TABLE IF NOT EXISTS sessions (
            id TEXT PRIMARY KEY,
            domain TEXT NOT NULL,
            started_at TEXT NOT NULL,
            completed_at TEXT,
            status TEXT NOT NULL,
            notes_count INTEGER DEFAULT 0,
            images_count INTEGER DEFAULT 0,
            split_mode INTEGER NOT NULL,
            timestamp_fmt TEXT NOT NULL,
            images_enabled INTEGER NOT NULL,
            output_path TEXT NOT NULL,
            images_dir_name TEXT,
            error_message TEXT
        );",
    )?;

    Ok(())
}

pub fn insert_session(db_path: &Path, session: &Session) -> AppResult<()> {
    let conn = Connection::open(db_path)?;
    conn.execute(
        "INSERT INTO sessions (
            id, domain, started_at, completed_at, status, notes_count, images_count,
            split_mode, timestamp_fmt, images_enabled, output_path, images_dir_name, error_message
        ) VALUES (?1, ?2, ?3, ?4, ?5, ?6, ?7, ?8, ?9, ?10, ?11, ?12, ?13)",
        params![
            session.id,
            session.domain,
            session.started_at,
            session.completed_at,
            session.status,
            session.notes_count as i64,
            session.images_count as i64,
            bool_to_i64(session.split_mode),
            session.timestamp_format,
            bool_to_i64(session.images_enabled),
            session.output_path,
            session.images_dir_name,
            session.error_message,
        ],
    )?;

    Ok(())
}

pub fn update_session_progress(
    db_path: &Path,
    session_id: &str,
    notes_count: u32,
    images_count: u32,
) -> AppResult<()> {
    let conn = Connection::open(db_path)?;
    conn.execute(
        "UPDATE sessions SET notes_count = ?1, images_count = ?2 WHERE id = ?3",
        params![notes_count as i64, images_count as i64, session_id],
    )?;

    Ok(())
}

pub fn set_session_outcome(
    db_path: &Path,
    session_id: &str,
    status: &str,
    completed_at: &str,
    notes_count: u32,
    images_count: u32,
    error_message: Option<&str>,
) -> AppResult<()> {
    let conn = Connection::open(db_path)?;
    conn.execute(
        "UPDATE sessions
         SET status = ?1, completed_at = ?2, notes_count = ?3, images_count = ?4, error_message = ?5
         WHERE id = ?6",
        params![
            status,
            completed_at,
            notes_count as i64,
            images_count as i64,
            error_message,
            session_id,
        ],
    )?;

    Ok(())
}

pub fn fetch_sessions(db_path: &Path, page: u32, per_page: u32) -> AppResult<Vec<Session>> {
    let safe_page = if page == 0 { 1 } else { page };
    let safe_per_page = if per_page == 0 { 50 } else { per_page.min(500) };
    let offset = (safe_page - 1) * safe_per_page;

    let conn = Connection::open(db_path)?;
    let mut stmt = conn.prepare(
        "SELECT
            id, domain, started_at, completed_at, status, notes_count, images_count,
            split_mode, timestamp_fmt, images_enabled, output_path, images_dir_name, error_message
         FROM sessions
         ORDER BY started_at DESC
         LIMIT ?1 OFFSET ?2",
    )?;

    let rows = stmt.query_map(
        params![safe_per_page as i64, offset as i64],
        map_session_row,
    )?;
    let mut sessions = Vec::new();
    for row in rows {
        sessions.push(row?);
    }

    Ok(sessions)
}

pub fn fetch_session_by_id(db_path: &Path, session_id: &str) -> AppResult<Option<Session>> {
    let conn = Connection::open(db_path)?;
    let mut stmt = conn.prepare(
        "SELECT
            id, domain, started_at, completed_at, status, notes_count, images_count,
            split_mode, timestamp_fmt, images_enabled, output_path, images_dir_name, error_message
         FROM sessions
         WHERE id = ?1
         LIMIT 1",
    )?;

    let session = stmt
        .query_row(params![session_id], map_session_row)
        .optional()?;

    Ok(session)
}

pub fn delete_session_by_id(db_path: &Path, session_id: &str) -> AppResult<Option<Session>> {
    let session = fetch_session_by_id(db_path, session_id)?;
    if session.is_none() {
        return Ok(None);
    }

    let conn = Connection::open(db_path)?;
    conn.execute("DELETE FROM sessions WHERE id = ?1", params![session_id])?;

    Ok(session)
}
