<script setup lang="ts">
import { computed } from "vue";
import ExportForm from "@/components/export/export-form.vue";
import ExportProgress from "@/components/export/export-progress.vue";
import { useExportStore } from "@/stores/export";
import type { StartExportPayload } from "@/types";

const exportStore = useExportStore();

const canStart = computed(() => !exportStore.isRunning);
const showProgress = computed(
  () => exportStore.logs.length > 0 && exportStore.statusText !== "Cancelled",
);

async function handleStart(payload: StartExportPayload) {
  if (!canStart.value) {
    return;
  }
  await exportStore.start(payload);
}
</script>

<template>
  <section class="grid gap-6">
    <ExportForm v-if="canStart" @start="handleStart" />
    <ExportProgress v-if="showProgress" />
  </section>
</template>
