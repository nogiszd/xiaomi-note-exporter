import { defineStore } from "pinia";
import { getVersion } from "@tauri-apps/api/app";
import { checkLatestReleaseVersion } from "@/lib/api";
import { compareVersions, normalizeVersion } from "@/lib/update";

interface UpdateState {
  currentVersion: string;
  latestVersion: string;
  checking: boolean;
  errorMessage: string;
  hasChecked: boolean;
  launchChecked: boolean;
  launchDialogOpen: boolean;
}

export const useUpdateStore = defineStore("update", {
  state: (): UpdateState => ({
    currentVersion: "",
    latestVersion: "",
    checking: false,
    errorMessage: "",
    hasChecked: false,
    launchChecked: false,
    launchDialogOpen: false,
  }),
  getters: {
    normalizedCurrentVersion(state): string {
      return normalizeVersion(state.currentVersion);
    },
    normalizedLatestVersion(state): string {
      return normalizeVersion(state.latestVersion);
    },
    comparisonResult(): number | null {
      if (!this.normalizedCurrentVersion || !this.normalizedLatestVersion) {
        return null;
      }

      return compareVersions(
        this.normalizedCurrentVersion,
        this.normalizedLatestVersion,
      );
    },
    hasUpdate(): boolean {
      return this.comparisonResult !== null && this.comparisonResult < 0;
    },
    upToDate(): boolean {
      return this.comparisonResult !== null && this.comparisonResult >= 0;
    },
  },
  actions: {
    async loadCurrentVersion() {
      if (this.currentVersion) {
        return;
      }

      try {
        this.currentVersion = normalizeVersion(await getVersion());
      } catch {
        this.currentVersion = "unknown";
      }
    },
    async checkUpdates() {
      if (this.checking) {
        return;
      }

      this.checking = true;
      this.hasChecked = false;
      this.errorMessage = "";

      try {
        await this.loadCurrentVersion();
        this.latestVersion = normalizeVersion(await checkLatestReleaseVersion());
        this.hasChecked = true;
      } catch (error) {
        this.errorMessage =
          error instanceof Error
            ? error.message
            : "Failed to check for updates.";
      } finally {
        this.checking = false;
      }
    },
    async checkUpdatesOnLaunch() {
      if (this.launchChecked) {
        return;
      }

      this.launchChecked = true;
      await this.checkUpdates();
      if (this.hasUpdate) {
        this.launchDialogOpen = true;
      }
    },
    setLaunchDialogOpen(value: boolean) {
      this.launchDialogOpen = value;
    },
  },
});
