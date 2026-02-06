<script setup lang="ts">
import { computed, ref } from "vue";
import * as yup from "yup";
import AppForm from "@/components/forms/app-form.vue";
import AppFormPathField from "@/components/forms/app-form-path-field.vue";
import { Button } from "@/components/ui/button";

const emit = defineEmits<{
  convert: [sourcePath: string, outputPath: string];
}>();

const converterSchema = yup.object({
  sourcePath: yup.string().trim().required("Source path is required."),
  outputPath: yup
    .string()
    .trim()
    .required("Output path is required.")
    .test("json-extension", "Output path must end with .json", (value) => {
      if (!value) {
        return false;
      }
      return value.toLowerCase().endsWith(".json");
    }),
});

const initialValues = {
  sourcePath: "",
  outputPath: "",
};
const sourceMode = ref<"file" | "directory">("file");
const sourceDescription = computed(() =>
  sourceMode.value === "file"
    ? "Pick a markdown file to convert."
    : "Pick a folder that contains exported markdown files.",
);
const sourcePlaceholder = computed(() =>
  sourceMode.value === "file"
    ? "C:\\Exports\\XiaomiNotes\\exported_notes_01-01-2026_12-00-00.md"
    : "C:\\Exports\\XiaomiNotes\\",
);

function handleSubmit(values: Record<string, unknown>) {
  const casted = converterSchema.cast(values, { stripUnknown: true }) as {
    sourcePath: string;
    outputPath: string;
  };

  emit("convert", casted.sourcePath.trim(), casted.outputPath.trim());
}
</script>

<template>
  <AppForm
    :schema="converterSchema"
    :initial-values="initialValues"
    title="Convert Markdown to JSON"
    description="Provide a markdown file or folder and choose where to write JSON output."
    submit-label="Convert"
    submitting-label="Converting..."
    @submit="handleSubmit"
  >
    <template #default>
      <div class="flex flex-wrap items-center gap-2">
        <Button
          type="button"
          size="sm"
          :variant="sourceMode === 'file' ? 'default' : 'outline'"
          @click="sourceMode = 'file'"
        >
          Source File
        </Button>
        <Button
          type="button"
          size="sm"
          :variant="sourceMode === 'directory' ? 'default' : 'outline'"
          @click="sourceMode = 'directory'"
        >
          Source Folder
        </Button>
      </div>
      <AppFormPathField
        name="sourcePath"
        label="Source markdown file or folder"
        :mode="sourceMode"
        dialog-title="Select source path"
        :placeholder="sourcePlaceholder"
        :description="sourceDescription"
        :filters="[{ name: 'Markdown', extensions: ['md', 'markdown'] }]"
      />
      <AppFormPathField
        name="outputPath"
        label="Output JSON path"
        mode="file"
        operation="save"
        dialog-title="Save output JSON file"
        placeholder="C:\\Exports\\XiaomiNotes\\exported_notes.json"
        description="Choose where the converted JSON file will be saved."
        :filters="[{ name: 'JSON', extensions: ['json'] }]"
      />
    </template>
  </AppForm>
</template>
