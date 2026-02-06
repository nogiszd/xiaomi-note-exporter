<script setup lang="ts">
import * as yup from "yup";
import AppForm from "@/components/forms/app-form.vue";
import AppFormInputField from "@/components/forms/app-form-input-field.vue";

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
      <AppFormInputField
        name="sourcePath"
        label="Source markdown file or folder"
        placeholder="C:\\Exports\\XiaomiNotes\\exported_notes_01-01-2026_12-00-00.md"
      />
      <AppFormInputField
        name="outputPath"
        label="Output JSON path"
        placeholder="C:\\Exports\\XiaomiNotes\\exported_notes.json"
      />
    </template>
  </AppForm>
</template>
