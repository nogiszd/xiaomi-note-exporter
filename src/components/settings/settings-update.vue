<script setup lang="ts">
import { openUrl } from "@tauri-apps/plugin-opener";
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

const updateStore = useUpdateStore();
const {
  checking,
  errorMessage,
  hasChecked,
  hasUpdate,
  upToDate,
  normalizedCurrentVersion,
  normalizedLatestVersion,
} = storeToRefs(updateStore);

async function checkUpdates() {
  await updateStore.checkUpdates();
}

async function openLatestReleasePage() {
  await openUrl(RELEASES_LATEST_URL);
}

onMounted(() => {
  void updateStore.loadCurrentVersion();
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
