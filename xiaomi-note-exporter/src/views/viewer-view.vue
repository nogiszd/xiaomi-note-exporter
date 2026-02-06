<script setup lang="ts">
import { computed, onMounted, ref, watch } from "vue";
import { useRoute, useRouter } from "vue-router";
import { MdEditor } from "md-editor-v3";
import "md-editor-v3/lib/style.css";
import { listExportFiles, readExportFile, writeExportFile } from "@/lib/api";
import { useSessionsStore } from "@/stores/sessions";
import { useSettingsStore } from "@/stores/settings";
import type { FileEntry } from "@/types";
import { Button } from "@/components/ui/button";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { ScrollArea } from "@/components/ui/scroll-area";

const route = useRoute();
const router = useRouter();
const sessionsStore = useSessionsStore();
const settingsStore = useSettingsStore();

const files = ref<FileEntry[]>([]);
const activeFilePath = ref("");
const content = ref("");
const loading = ref(false);
const errorMessage = ref("");
const saveStatus = ref("");
const editorTheme = computed(() =>
  settingsStore.settings.darkMode ? "dark" : "light",
);

const sessionId = computed(() => {
  const param = route.params.sessionId;
  return typeof param === "string" ? param : "";
});

const activeFile = computed(
  () =>
    files.value.find((entry) => entry.path === activeFilePath.value) || null,
);

async function loadActiveFile() {
  if (!activeFilePath.value) {
    content.value = "";
    return;
  }
  content.value = await readExportFile(activeFilePath.value);
}

async function loadSessionFiles() {
  if (!sessionId.value) {
    files.value = [];
    return;
  }

  loading.value = true;
  errorMessage.value = "";
  saveStatus.value = "";

  try {
    const session = await sessionsStore.findById(sessionId.value);
    if (!session) {
      files.value = [];
      return;
    }

    try {
      files.value = await listExportFiles(session.outputPath);
    } catch {
      files.value = [
        {
          name: session.outputPath.split(/[\\/]/).pop() || session.outputPath,
          path: session.outputPath,
        },
      ];
    }

    activeFilePath.value = files.value[0]?.path || "";
    await loadActiveFile();
  } catch (error) {
    errorMessage.value =
      error instanceof Error ? error.message : "Failed to load files.";
  } finally {
    loading.value = false;
  }
}

async function saveFile() {
  if (!activeFilePath.value) {
    return;
  }

  saveStatus.value = "Saving...";
  try {
    await writeExportFile(activeFilePath.value, content.value);
    saveStatus.value = "Saved.";
  } catch (error) {
    saveStatus.value = error instanceof Error ? error.message : "Save failed.";
  }
}

function goToConverter() {
  void router.push("/converter");
}

watch(
  () => sessionId.value,
  () => {
    void loadSessionFiles();
  },
);

onMounted(() => {
  void loadSessionFiles();
});
</script>

<template>
  <section class="grid gap-4 lg:grid-cols-[320px_minmax(0,1fr)]">
    <Card>
      <CardHeader>
        <CardTitle class="text-base">Files</CardTitle>
      </CardHeader>
      <CardContent>
        <ScrollArea class="h-135 pr-2">
          <div class="grid gap-2">
            <Button
              v-for="entry in files"
              :key="entry.path"
              type="button"
              :variant="entry.path === activeFilePath ? 'secondary' : 'ghost'"
              class="justify-start truncate"
              @click="
                activeFilePath = entry.path;
                loadActiveFile();
              "
            >
              {{ entry.name }}
            </Button>
          </div>
        </ScrollArea>
      </CardContent>
    </Card>

    <div class="grid gap-4">
      <div class="flex flex-wrap items-center gap-2">
        <Button type="button" @click="saveFile">Save</Button>
        <Button type="button" variant="outline" @click="goToConverter"
          >Export to JSON</Button
        >
        <span v-if="saveStatus" class="text-xs text-muted-foreground">{{
          saveStatus
        }}</span>
      </div>

      <Card v-if="loading">
        <CardContent class="pt-6 text-sm text-muted-foreground"
          >Loading files...</CardContent
        >
      </Card>
      <Card v-else-if="errorMessage" class="border-destructive/50">
        <CardContent class="pt-6 text-sm text-destructive">{{
          errorMessage
        }}</CardContent>
      </Card>
      <Card v-else-if="!activeFile">
        <CardContent class="pt-6 text-sm text-muted-foreground"
          >No file selected.</CardContent
        >
      </Card>
      <MdEditor
        v-else
        v-model="content"
        :theme="editorTheme"
        preview-theme="github"
        code-theme="atom"
        language="en-US"
        :style="{ height: '70vh' }"
        @on-save="saveFile"
      />
    </div>
  </section>
</template>
