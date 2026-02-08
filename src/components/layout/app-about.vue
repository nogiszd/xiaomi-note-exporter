<script setup lang="ts">
import logo from "@/assets/logo.png";
import { Badge } from "@/components/ui/badge";
import {
  AlertDialog,
  AlertDialogCancel,
  AlertDialogContent,
  AlertDialogDescription,
  AlertDialogFooter,
  AlertDialogHeader,
  AlertDialogTitle,
} from "@/components/ui/alert-dialog";

defineProps<{
  open: boolean;
}>();

const emit = defineEmits<{
  (event: "update:open", value: boolean): void;
}>();

const appName = "Xiaomi Note Exporter";
const appVersion = "2.0.0";
const appLicense = "GNU General Public License v3.0 (GPL-3.0)";

const libraries = [
  { name: "Tauri", license: "MIT" },
  { name: "Vue 3", license: "MIT" },
  { name: "Vite", license: "MIT" },
  { name: "Tailwind CSS", license: "MIT" },
  { name: "shadcn-vue", license: "MIT" },
  { name: "Pinia", license: "MIT" },
  { name: "Vue Router", license: "MIT" },
  { name: "md-editor-v3", license: "MIT" },
  { name: "rusqlite", license: "MIT" },
  { name: "reqwest", license: "MIT" },
  { name: "chrono", license: "MIT" },
] as const;

function onOpenChange(value: boolean) {
  emit("update:open", value);
}
</script>

<template>
  <AlertDialog :open="open" @update:open="onOpenChange">
    <AlertDialogContent class="sm:max-w-xl">
      <AlertDialogHeader class="items-center text-center">
        <img
          :src="logo"
          alt="Xiaomi Note Exporter logo"
          class="size-16 rounded-2xl"
        />
        <AlertDialogTitle>{{ appName }}</AlertDialogTitle>
        <AlertDialogDescription
          >Version {{ appVersion }}</AlertDialogDescription
        >
      </AlertDialogHeader>

      <div class="grid gap-4 text-sm">
        <div class="grid gap-1">
          <p class="text-xs uppercase tracking-wide text-muted-foreground">
            License
          </p>
          <p class="font-medium">{{ appLicense }}</p>
        </div>

        <div class="grid gap-2">
          <p class="text-xs uppercase tracking-wide text-muted-foreground">
            Used libraries
          </p>
          <div class="flex flex-wrap gap-2 bg-accent p-2 rounded-lg">
            <Badge
              v-for="library in libraries"
              :key="library.name"
              variant="outline"
            >
              {{ library.name }} - {{ library.license }}
            </Badge>
          </div>
        </div>
      </div>

      <AlertDialogFooter>
        <AlertDialogCancel>Close</AlertDialogCancel>
      </AlertDialogFooter>
    </AlertDialogContent>
  </AlertDialog>
</template>
