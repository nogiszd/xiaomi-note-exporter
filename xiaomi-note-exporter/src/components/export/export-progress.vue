<script setup lang="ts">
import { computed } from "vue";
import { openInExplorer } from "@/lib/api";
import { useExportStore } from "@/stores/export";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import {
  Card,
  CardContent,
  CardFooter,
  CardHeader,
  CardTitle,
} from "@/components/ui/card";
import { Progress } from "@/components/ui/progress";
import { ScrollArea } from "@/components/ui/scroll-area";

const exportStore = useExportStore();

const elapsedSeconds = computed(() => Math.floor(exportStore.elapsedMs / 1000));
const elapsedFormatted = computed(() => {
  const minutes = Math.floor(elapsedSeconds.value / 60);
  const seconds = elapsedSeconds.value % 60;
  return `${String(minutes).padStart(2, "0")}:${String(seconds).padStart(2, "0")}`;
});

async function openFolder() {
  if (!exportStore.outputPath) {
    return;
  }
  await openInExplorer(exportStore.outputPath);
}
</script>

<template>
  <Card>
    <CardHeader class="space-y-3">
      <div class="flex items-center justify-between gap-3">
        <CardTitle>{{ exportStore.statusText }}</CardTitle>
        <Badge variant="outline">{{ exportStore.progressPercent }}%</Badge>
      </div>
      <Progress :model-value="exportStore.progressPercent" />
      <p class="text-sm text-muted-foreground">
        {{ exportStore.current }} / {{ exportStore.total || "?" }} notes,
        {{ exportStore.imagesCount }} images, elapsed:
        {{ elapsedFormatted }}
      </p>
    </CardHeader>

    <CardContent class="space-y-3">
      <p class="text-sm font-medium">Live Log</p>
      <ScrollArea class="h-52 rounded-md border p-3">
        <ul class="space-y-1 text-sm">
          <li
            v-for="(line, index) in exportStore.logs"
            :key="`${index}-${line}`"
            class="text-muted-foreground"
          >
            {{ line }}
          </li>
        </ul>
      </ScrollArea>
    </CardContent>

    <CardFooter>
      <Button
        v-if="exportStore.isRunning"
        type="button"
        variant="destructive"
        @click="exportStore.cancel()"
      >
        Cancel
      </Button>
      <Button v-else type="button" variant="outline" @click="openFolder"
        >Open Folder</Button
      >
    </CardFooter>
  </Card>
</template>
