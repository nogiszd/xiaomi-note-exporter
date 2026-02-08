import { defineStore } from "pinia";
import { listen, type UnlistenFn } from "@tauri-apps/api/event";
import { cancelExport, startExport } from "@/lib/api";
import type {
  ExportCompleteEvent,
  ExportErrorEvent,
  ExportProgressEvent,
  StartExportPayload,
} from "@/types";

interface ExportState {
  isRunning: boolean;
  activeSessionId: string | null;
  current: number;
  total: number;
  notesCount: number;
  imagesCount: number;
  startedAtMs: number | null;
  elapsedMs: number;
  outputPath: string;
  statusText: string;
  logs: string[];
}

let unlisteners: UnlistenFn[] = [];
let listenersReady = false;

export const useExportStore = defineStore("export", {
  state: (): ExportState => ({
    isRunning: false,
    activeSessionId: null,
    current: 0,
    total: 0,
    notesCount: 0,
    imagesCount: 0,
    startedAtMs: null,
    elapsedMs: 0,
    outputPath: "",
    statusText: "Idle",
    logs: [],
  }),
  getters: {
    progressPercent(state): number {
      if (state.total <= 0) {
        return 0;
      }
      return Math.round((state.current / state.total) * 100);
    },
  },
  actions: {
    async initializeListeners() {
      if (listenersReady) {
        return;
      }

      const progressUnlisten = await listen<ExportProgressEvent>("export:progress", (event) => {
        const payload = event.payload;
        this.isRunning = true;
        this.activeSessionId = payload.sessionId;
        this.current = payload.current;
        this.total = payload.total;
        this.notesCount = payload.notesCount;
        this.imagesCount = payload.imagesCount;
        this.statusText = `Exporting ${payload.current}/${payload.total}`;
        this.logs = [payload.logLine, ...this.logs].slice(0, 100);
        if (this.startedAtMs) {
          this.elapsedMs = Date.now() - this.startedAtMs;
        }
      });

      const completeUnlisten = await listen<ExportCompleteEvent>("export:complete", (event) => {
        const payload = event.payload;
        this.isRunning = false;
        this.statusText = "Completed";
        this.outputPath = payload.outputPath;
        this.elapsedMs = payload.elapsedMs;
        this.logs = [`Export completed (${payload.total} notes).`, ...this.logs].slice(0, 100);
      });

      const errorUnlisten = await listen<ExportErrorEvent>("export:error", (event) => {
        const payload = event.payload;
        this.isRunning = false;
        this.statusText = "Error";
        this.logs = [`Error: ${payload.message}`, ...this.logs].slice(0, 100);
      });

      unlisteners = [progressUnlisten, completeUnlisten, errorUnlisten];
      listenersReady = true;
    },
    async start(payload: StartExportPayload) {
      this.startedAtMs = Date.now();
      this.elapsedMs = 0;
      this.current = 0;
      this.total = 0;
      this.notesCount = 0;
      this.imagesCount = 0;
      this.outputPath = payload.outputDir;
      this.statusText = "Starting...";
      this.logs = ["Starting export..."];
      this.activeSessionId = await startExport(payload);
      this.isRunning = true;
    },
    async cancel() {
      await cancelExport();
      this.isRunning = false;
      this.statusText = "Cancelled";
      this.logs = ["Export cancelled by user.", ...this.logs].slice(0, 100);
    },
    disposeListeners() {
      for (const unlisten of unlisteners) {
        unlisten();
      }
      unlisteners = [];
      listenersReady = false;
    },
  },
});
