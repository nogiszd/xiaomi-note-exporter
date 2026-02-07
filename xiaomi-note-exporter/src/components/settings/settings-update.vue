<script setup lang="ts">
import { computed, onMounted, ref } from "vue";
import { getVersion } from "@tauri-apps/api/app";
import { openUrl } from "@tauri-apps/plugin-opener";
import { checkLatestReleaseVersion } from "@/lib/api";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import {
  Card,
  CardAction,
  CardContent,
  CardDescription,
  CardHeader,
  CardTitle,
} from "@/components/ui/card";

const RELEASES_LATEST_URL =
  "https://github.com/nogiszd/xiaomi-note-exporter/releases/latest";

const currentVersion = ref("");
const latestVersion = ref("");
const checking = ref(false);
const errorMessage = ref("");
const hasChecked = ref(false);

function normalizeVersion(value: string): string {
  return value.trim().replace(/^v/i, "");
}

function parseVersionParts(version: string): number[] | null {
  const normalized = normalizeVersion(version);
  if (!normalized) {
    return null;
  }

  const parts = normalized.split(".");
  if (parts.length === 0) {
    return null;
  }

  const parsed = parts.map((part) => Number.parseInt(part, 10));
  if (parsed.some((part) => !Number.isFinite(part))) {
    return null;
  }

  return parsed;
}

function compareVersions(left: string, right: string): number | null {
  const leftParts = parseVersionParts(left);
  const rightParts = parseVersionParts(right);
  if (!leftParts || !rightParts) {
    return null;
  }

  const maxLength = Math.max(leftParts.length, rightParts.length);

  for (let index = 0; index < maxLength; index += 1) {
    const leftValue = leftParts[index] ?? 0;
    const rightValue = rightParts[index] ?? 0;
    if (leftValue > rightValue) {
      return 1;
    }
    if (leftValue < rightValue) {
      return -1;
    }
  }

  return 0;
}

const normalizedCurrentVersion = computed(() =>
  normalizeVersion(currentVersion.value),
);
const normalizedLatestVersion = computed(() =>
  normalizeVersion(latestVersion.value),
);
const comparisonResult = computed(() => {
  if (!normalizedCurrentVersion.value || !normalizedLatestVersion.value) {
    return null;
  }
  return compareVersions(
    normalizedCurrentVersion.value,
    normalizedLatestVersion.value,
  );
});
const hasUpdate = computed(
  () => comparisonResult.value !== null && comparisonResult.value < 0,
);
const upToDate = computed(
  () => comparisonResult.value !== null && comparisonResult.value >= 0,
);

async function loadCurrentVersion() {
  try {
    currentVersion.value = normalizeVersion(await getVersion());
  } catch {
    currentVersion.value = "unknown";
  }
}

async function checkUpdates() {
  checking.value = true;
  hasChecked.value = false;
  errorMessage.value = "";

  try {
    latestVersion.value = normalizeVersion(await checkLatestReleaseVersion());
    hasChecked.value = true;
  } catch (error) {
    errorMessage.value =
      error instanceof Error ? error.message : "Failed to check for updates.";
  } finally {
    checking.value = false;
  }
}

async function openLatestReleasePage() {
  await openUrl(RELEASES_LATEST_URL);
}

onMounted(() => {
  void loadCurrentVersion();
});
</script>

<template>
  <Card>
    <CardHeader>
      <CardTitle>Check for updates</CardTitle>
      <CardDescription
        >Check for the latest version of the application.</CardDescription
      >
      <CardAction>
        <Badge v-if="hasChecked && hasUpdate" variant="secondary" class="w-fit">
          New version available
        </Badge>
        <Badge
          v-else-if="hasChecked && upToDate"
          variant="outline"
          class="w-fit"
        >
          You are up to date!
        </Badge>
      </CardAction>
    </CardHeader>
    <CardContent class="grid gap-4">
      <div class="flex items-center gap-12 pb-2">
        <div class="grid gap-1 text-sm">
          <p class="text-muted-foreground">Current version</p>
          <p class="font-medium">{{ normalizedCurrentVersion || "" }}</p>
        </div>

        <div class="grid gap-1 text-sm">
          <p class="text-muted-foreground">Latest version</p>
          <p class="font-medium">
            {{ hasChecked ? normalizedLatestVersion : "Not checked yet" }}
          </p>
        </div>
      </div>

      <div v-if="errorMessage" class="text-xs text-destructive">
        {{ errorMessage }}
      </div>

      <div class="flex flex-wrap items-center gap-2">
        <Button type="button" :disabled="checking" @click="checkUpdates">
          {{ checking ? "Checking..." : "Check updates" }}
        </Button>
        <Button type="button" variant="outline" @click="openLatestReleasePage">
          Open latest release
        </Button>
      </div>
    </CardContent>
  </Card>
</template>
