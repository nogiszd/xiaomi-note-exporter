<script setup lang="ts">
import { openUrl } from "@tauri-apps/plugin-opener";
import {
  AlertDialog,
  AlertDialogAction,
  AlertDialogCancel,
  AlertDialogContent,
  AlertDialogDescription,
  AlertDialogFooter,
  AlertDialogHeader,
  AlertDialogTitle,
} from "@/components/ui/alert-dialog";

const updateStore = useUpdateStore();
const { launchDialogOpen, normalizedCurrentVersion, normalizedLatestVersion } =
  storeToRefs(updateStore);

function onOpenChange(value: boolean) {
  updateStore.setLaunchDialogOpen(value);
}

async function openLatestReleasePage() {
  await openUrl(RELEASES_LATEST_URL);
}
</script>

<template>
  <AlertDialog :open="launchDialogOpen" @update:open="onOpenChange">
    <AlertDialogContent>
      <AlertDialogHeader>
        <AlertDialogTitle>Update available</AlertDialogTitle>
        <AlertDialogDescription>
          A newer version of Xiaomi Note Exporter is available.
        </AlertDialogDescription>
      </AlertDialogHeader>

      <div class="grid gap-1 text-sm">
        <p class="text-muted-foreground">
          Current version: {{ normalizedCurrentVersion || "unknown" }}
        </p>
        <p class="text-muted-foreground">
          Latest version:
          <span class="font-bold">{{ normalizedLatestVersion }}</span>
        </p>
      </div>

      <AlertDialogFooter>
        <AlertDialogCancel>Later</AlertDialogCancel>
        <AlertDialogAction @click="openLatestReleasePage">
          Open latest release
        </AlertDialogAction>
      </AlertDialogFooter>
    </AlertDialogContent>
  </AlertDialog>
</template>
