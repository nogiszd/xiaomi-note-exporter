import { defineStore } from "pinia";
import { getAppSettings, updateAppSettings } from "@/lib/api";
import type { AppSettings } from "@/types";

const FALLBACK_SETTINGS: AppSettings = {
  defaultExportDir: "",
  darkMode: false,
};

interface SettingsState {
  loaded: boolean;
  loading: boolean;
  error: string;
  current: AppSettings;
}

export const useSettingsStore = defineStore("settings", {
  state: (): SettingsState => ({
    loaded: false,
    loading: false,
    error: "",
    current: { ...FALLBACK_SETTINGS },
  }),
  getters: {
    settings(state): AppSettings {
      return state.current;
    },
  },
  actions: {
    applyTheme(darkMode: boolean) {
      document.documentElement.classList.toggle("dark", darkMode);
    },
    async load() {
      if (this.loading || this.loaded) {
        return;
      }

      this.loading = true;
      this.error = "";
      try {
        const settings = await getAppSettings();
        this.current = settings;
        this.loaded = true;
        this.applyTheme(settings.darkMode);
      } catch (error) {
        this.error = error instanceof Error ? error.message : "Failed to load settings.";
        this.loaded = true;
        this.applyTheme(this.current.darkMode);
      } finally {
        this.loading = false;
      }
    },
    async save(settings: AppSettings) {
      this.loading = true;
      this.error = "";
      try {
        const updated = await updateAppSettings(settings);
        this.current = updated;
        this.applyTheme(updated.darkMode);
      } catch (error) {
        this.error = error instanceof Error ? error.message : "Failed to save settings.";
        throw error;
      } finally {
        this.loading = false;
      }
    },
  },
});
