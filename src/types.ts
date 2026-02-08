export type SessionStatus = "running" | "completed" | "cancelled" | "error";

export interface Session {
  id: string;
  domain: string;
  startedAt: string;
  completedAt: string | null;
  status: SessionStatus;
  notesCount: number;
  imagesCount: number;
  splitMode: boolean;
  timestampFormat: string;
  imagesEnabled: boolean;
  outputPath: string;
  imagesDirName: string | null;
  errorMessage: string | null;
}

export interface FileEntry {
  name: string;
  path: string;
}

export interface StartExportPayload {
  domain: string;
  outputDir: string;
  split: boolean;
  timestampFormat: string;
  exportImages: boolean;
}

export interface ExportProgressEvent {
  sessionId: string;
  current: number;
  total: number;
  lastTitle: string;
  notesCount: number;
  imagesCount: number;
  logLine: string;
}

export interface ExportCompleteEvent {
  sessionId: string;
  total: number;
  elapsedMs: number;
  outputPath: string;
}

export interface ExportErrorEvent {
  sessionId: string;
  message: string;
}

export interface NoteDto {
  id: string;
  content: string;
  creationDate: string;
  lastModified: string;
}

export interface AppSettings {
  defaultExportDir: string;
  theme: "system" | "light" | "dark";
}
