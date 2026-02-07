<script setup lang="ts">
import { computed } from "vue";
import { useRoute } from "vue-router";
import {
  Download,
  FileJson2,
  FileText,
  History,
  Settings,
} from "lucide-vue-next";
import { SidebarTrigger } from "@/components/ui/sidebar";
import { Separator } from "@/components/ui/separator";

const route = useRoute();

const headerMeta = computed(() => {
  if (route.path.startsWith("/export")) {
    return { title: "Export Notes", icon: Download };
  }
  if (route.path.startsWith("/history")) {
    return { title: "Export History", icon: History };
  }
  if (route.path.startsWith("/converter")) {
    return { title: "Markdown to JSON Converter", icon: FileJson2 };
  }
  if (route.path.startsWith("/viewer")) {
    return { title: "Markdown Viewer", icon: FileText };
  }
  if (route.path.startsWith("/settings")) {
    return { title: "Settings", icon: Settings };
  }
  return { title: "Xiaomi Note Exporter", icon: FileText };
});
</script>

<template>
  <header
    class="sticky top-0 z-10 border-b bg-background/95 px-2 py-3 backdrop-blur md:px-4 select-none"
  >
    <div class="flex items-center h-full space-x-3">
      <SidebarTrigger />
      <Separator orientation="vertical" />
      <h2 class="flex items-center gap-2 text-lg font-semibold tracking-tight">
        <component :is="headerMeta.icon" class="size-5 text-muted-foreground" />
        <span>{{ headerMeta.title }}</span>
      </h2>
    </div>
  </header>
</template>
