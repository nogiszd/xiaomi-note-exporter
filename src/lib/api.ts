import { invoke } from "@tauri-apps/api/core";
import type {
  AppSettings,
  FileEntry,
  Session,
  StartExportPayload,
} from "@/types";

export async function startExport(
  payload: StartExportPayload,
): Promise<string> {
  return invoke<string>("start_export", {
    domain: payload.domain,
    outputDir: payload.outputDir,
    split: payload.split,
    timestampFormat: payload.timestampFormat,
    exportImages: payload.exportImages,
  });
}

export async function cancelExport(): Promise<void> {
  return invoke<void>("cancel_export");
}

export async function getSessions(page = 1, perPage = 50): Promise<Session[]> {
  return invoke<Session[]>("get_sessions", { page, perPage });
}

export async function getSession(id: string): Promise<Session | null> {
  return invoke<Session | null>("get_session", { id });
}

export async function deleteSession(
  id: string,
  deleteFiles: boolean,
): Promise<void> {
  return invoke<void>("delete_session", { id, deleteFiles });
}

export async function readExportFile(path: string): Promise<string> {
  return invoke<string>("read_export_file", { path });
}

export async function writeExportFile(
  path: string,
  content: string,
): Promise<void> {
  return invoke<void>("write_export_file", { path, content });
}

export async function listExportFiles(dirPath: string): Promise<FileEntry[]> {
  return invoke<FileEntry[]>("list_export_files", { dirPath });
}

export async function convertToJson(
  sourcePath: string,
  outputPath: string,
): Promise<string> {
  return invoke<string>("convert_to_json", {
    sourcePath,
    outputPath,
  });
}

export async function openInExplorer(path: string): Promise<void> {
  return invoke<void>("open_in_explorer", { path });
}

export async function getAppSettings(): Promise<AppSettings> {
  return invoke<AppSettings>("get_app_settings");
}

export async function updateAppSettings(
  settings: AppSettings,
): Promise<AppSettings> {
  return invoke<AppSettings>("update_app_settings", {
    settingsPayload: settings,
  });
}

export async function checkLatestReleaseVersion(): Promise<string> {
  return invoke<string>("check_latest_release_version");
}

export async function closeSplashscreen(): Promise<void> {
  return invoke<void>("close_splashscreen");
}
