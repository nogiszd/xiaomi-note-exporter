use std::{collections::BTreeSet, path::Path};

use rusqlite::{params, Connection};

use crate::error::{AppError, AppResult};

struct Migration {
    version: i64,
    name: &'static str,
    sql: &'static str,
}

const MIGRATIONS: &[Migration] = &[Migration {
    version: 1,
    name: "0001_create_sessions_table",
    sql: "CREATE TABLE IF NOT EXISTS sessions (
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
}];
// Add new migrations here with strictly increasing versions.
// Example: Migration { version: 2, name: "0002_add_new_column", sql: "ALTER TABLE ..." }

fn validate_migrations() -> AppResult<()> {
    let mut prev_version = 0;
    for migration in MIGRATIONS {
        if migration.version <= prev_version {
            return Err(AppError::Message(format!(
                "Invalid migration order: version {} ('{}') must be greater than {}.",
                migration.version, migration.name, prev_version
            )));
        }
        prev_version = migration.version;
    }
    Ok(())
}

fn apply_migrations(conn: &mut Connection) -> AppResult<()> {
    validate_migrations()?;

    let tx = conn.transaction()?;
    tx.execute_batch(
        "CREATE TABLE IF NOT EXISTS schema_migrations (
            version INTEGER PRIMARY KEY,
            name TEXT NOT NULL,
            applied_at TEXT NOT NULL DEFAULT (strftime('%Y-%m-%dT%H:%M:%fZ', 'now'))
        );",
    )?;

    let applied_versions = {
        let mut stmt = tx.prepare("SELECT version FROM schema_migrations")?;
        let rows = stmt.query_map([], |row| row.get::<_, i64>(0))?;
        let mut versions = BTreeSet::new();
        for row in rows {
            versions.insert(row?);
        }
        versions
    };

    for migration in MIGRATIONS {
        if applied_versions.contains(&migration.version) {
            continue;
        }

        tx.execute_batch(migration.sql)?;
        tx.execute(
            "INSERT INTO schema_migrations (version, name) VALUES (?1, ?2)",
            params![migration.version, migration.name],
        )?;
    }

    tx.commit()?;
    Ok(())
}

pub fn run_migrations(db_path: &Path) -> AppResult<()> {
    let mut conn = Connection::open(db_path)?;
    apply_migrations(&mut conn)
}
