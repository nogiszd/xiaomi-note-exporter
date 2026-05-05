<script setup lang="ts">
import { convertFileSrc } from "@tauri-apps/api/core";
import { MdEditor } from "md-editor-v3";
import { ChevronLeft } from "lucide-vue-next";

import type { FileEntry } from "@/types";
import { Button } from "@/components/ui/button";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { ScrollArea } from "@/components/ui/scroll-area";

import "md-editor-v3/lib/style.css";

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
  settingsStore.isDarkTheme ? "dark" : "light",
);

const sessionId = computed(() => {
  const param = route.params.sessionId;
  return typeof param === "string" ? param : "";
});

const activeFile = computed(
  () =>
    files.value.find((entry) => entry.path === activeFilePath.value) || null,
);

function resolveImageSource(src: string) {
  const raw = src.trim();
  if (!raw || isRemoteOrSpecialSource(raw)) {
    return raw;
  }

  const decoded = (() => {
    try {
      return decodeURIComponent(raw);
    } catch {
      return raw;
    }
  })();

  const isAbsolutePath =
    /^[a-zA-Z]:[\\/]/.test(decoded) ||
    decoded.startsWith("/") ||
    decoded.startsWith("\\");

  const resolvedPath =
    isAbsolutePath || !activeFilePath.value
      ? toNativeFilePath(decoded)
      : resolveRelativePath(activeFilePath.value, decoded);

  return convertFileSrc(resolvedPath);
}

function sanitizePreviewHtml(html: string) {
  const document = new DOMParser().parseFromString(html, "text/html");
  for (const image of document.querySelectorAll("img[src]")) {
    const source = image.getAttribute("src") ?? "";
    image.setAttribute("src", resolveImageSource(source));
  }
  return document.body.innerHTML;
}

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
      const entries = await listExportFiles(session.outputPath);
      files.value = entries.filter((entry) =>
        entry.name.toLowerCase().endsWith(".md"),
      );
    } catch {
      const fallbackName =
        session.outputPath.split(/[\\/]/).pop() || session.outputPath;
      files.value = fallbackName.toLowerCase().endsWith(".md")
        ? [{ name: fallbackName, path: session.outputPath }]
        : [];
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
  void router.push({
    name: "converter",
    query: activeFilePath.value ? { sourcePath: activeFilePath.value } : {},
  });
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
  <section class="grid h-full min-h-0 gap-4 lg:grid-cols-[240px_minmax(0,1fr)]">
    <Card class="flex h-full min-h-0 flex-col overflow-hidden">
      <CardHeader>
        <CardTitle class="text-base">Files</CardTitle>
      </CardHeader>
      <CardContent class="min-h-0 flex-1 overflow-hidden">
        <ScrollArea class="h-full">
          <div class="grid gap-2">
            <Button
              v-for="entry in files"
              :key="entry.path"
              type="button"
              :variant="entry.path === activeFilePath ? 'secondary' : 'ghost'"
              class="justify-start truncate text-right text-rtl"
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

    <div class="flex h-full min-h-0 flex-col gap-4">
      <div class="flex flex-wrap items-center justify-between gap-2">
        <div class="flex flex-wrap items-center gap-2">
          <Button type="button" @click="saveFile">Save</Button>
          <Button type="button" variant="outline" @click="goToConverter">
            Export to JSON
          </Button>
          <span v-if="saveStatus" class="text-xs text-muted-foreground">
            {{ saveStatus }}
          </span>
        </div>
        <Button type="button" variant="outline" @click="router.back()">
          <ChevronLeft /> Go Back
        </Button>
      </div>

      <Card v-if="loading" class="flex-1">
        <CardContent class="pt-6 text-sm text-muted-foreground">
          Loading files...
        </CardContent>
      </Card>
      <Card v-else-if="errorMessage" class="flex-1 border-destructive/50">
        <CardContent class="pt-6 text-sm text-destructive">
          {{ errorMessage }}
        </CardContent>
      </Card>
      <Card v-else-if="!activeFile" class="flex-1">
        <CardContent class="pt-6 text-sm text-muted-foreground">
          No file selected.
        </CardContent>
      </Card>
      <div v-else class="min-h-0 flex-1">
        <MdEditor
          v-model="content"
          :theme="editorTheme"
          preview-theme="github"
          code-theme="atom"
          language="en-US"
          :sanitize="sanitizePreviewHtml"
          :toolbars-exclude="['fullscreen', 'save', 'htmlPreview', 'github']"
          :style="{ height: '100%', borderRadius: '0.475rem' }"
          @on-save="saveFile"
        />
      </div>
    </div>
  </section>
</template>
