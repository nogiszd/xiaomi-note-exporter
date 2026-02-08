<script setup lang="ts">
import { computed, ref } from "vue";
import { useField } from "vee-validate";
import { FolderOpen, FileSearch, X } from "lucide-vue-next";
import { open, save, type DialogFilter } from "@tauri-apps/plugin-dialog";
import {
  Field,
  FieldContent,
  FieldDescription,
  FieldError,
  FieldLabel,
} from "@/components/ui/field";
import { Input } from "@/components/ui/input";
import { Button } from "@/components/ui/button";

const props = withDefaults(
  defineProps<{
    name: string;
    label: string;
    mode?: "directory" | "file";
    operation?: "open" | "save";
    placeholder?: string;
    description?: string;
    dialogTitle?: string;
    filters?: DialogFilter[];
    disabled?: boolean;
  }>(),
  {
    mode: "directory",
    operation: "open",
    placeholder: "",
    description: "",
    dialogTitle: "",
    filters: () => [],
    disabled: false,
  },
);

const { value, errorMessage, handleBlur, setValue } = useField<string>(() => props.name);
const fieldId = computed(() => `field-${props.name.replace(/[^a-zA-Z0-9_-]/g, "-")}`);
const isPicking = ref(false);

const browseLabel = computed(() => {
  if (props.mode === "directory") {
    return "Browse Folder";
  }
  return props.operation === "save" ? "Save As" : "Browse File";
});
const browseIcon = computed(() => (props.mode === "directory" ? FolderOpen : FileSearch));
const resolvedDialogTitle = computed(() => {
  if (props.dialogTitle.trim()) {
    return props.dialogTitle;
  }
  if (props.mode === "directory") {
    return "Select folder";
  }
  return props.operation === "save" ? "Save file as" : "Select file";
});

async function browsePath() {
  if (props.disabled || isPicking.value) {
    return;
  }

  isPicking.value = true;
  try {
    const selection =
      props.mode === "file" && props.operation === "save"
        ? await save({
            title: resolvedDialogTitle.value,
            defaultPath: value.value || undefined,
            filters: props.filters,
          })
        : await open({
            title: resolvedDialogTitle.value,
            defaultPath: value.value || undefined,
            directory: props.mode === "directory",
            multiple: false,
            filters: props.mode === "file" ? props.filters : undefined,
          });

    if (typeof selection === "string") {
      setValue(selection);
      handleBlur();
    }
  } finally {
    isPicking.value = false;
  }
}

function clearPath() {
  setValue("");
  handleBlur();
}
</script>

<template>
  <Field>
    <FieldLabel :for="fieldId">{{ label }}</FieldLabel>
    <FieldContent>
      <div class="flex gap-2">
        <Input
          :id="fieldId"
          :model-value="value"
          :placeholder="placeholder"
          :disabled="disabled"
          autocomplete="off"
          @update:model-value="setValue(String($event))"
          @blur="handleBlur"
        />
        <Button
          type="button"
          variant="outline"
          :disabled="disabled || isPicking"
          @click="browsePath"
        >
          <component :is="browseIcon" class="size-4" />
          <span class="hidden sm:inline">{{ browseLabel }}</span>
        </Button>
        <Button
          v-if="value"
          type="button"
          variant="ghost"
          size="icon"
          :disabled="disabled || isPicking"
          @click="clearPath"
        >
          <X class="size-4" />
          <span class="sr-only">Clear path</span>
        </Button>
      </div>
      <FieldDescription v-if="description">{{ description }}</FieldDescription>
      <FieldError :errors="errorMessage ? [errorMessage] : []" />
    </FieldContent>
  </Field>
</template>
