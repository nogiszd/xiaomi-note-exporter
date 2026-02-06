<script setup lang="ts">
import * as yup from "yup";
import AppForm from "@/components/forms/app-form.vue";
import AppFormInputField from "@/components/forms/app-form-input-field.vue";
import AppFormSwitchField from "@/components/forms/app-form-switch-field.vue";
import type { StartExportPayload } from "@/types";

const emit = defineEmits<{
  start: [payload: StartExportPayload];
}>();

const defaultTimestampFormat = "dd-MM-yyyy_HH-mm-ss";

const exportSchema = yup.object({
  domain: yup
    .string()
    .trim()
    .required("Domain is required.")
    .matches(/^([a-z0-9-]+\.)+[a-z]{2,}$/i, "Enter a valid domain like us.i.mi.com."),
  split: yup.boolean().required(),
  timestampFormat: yup.string().when("split", {
    is: true,
    then: (schema) => schema.trim().required("Timestamp format is required when split mode is enabled."),
    otherwise: (schema) => schema.default(defaultTimestampFormat),
  }),
  exportImages: yup.boolean().required(),
});

const initialValues = {
  domain: "us.i.mi.com",
  split: false,
  timestampFormat: defaultTimestampFormat,
  exportImages: true,
};

function handleSubmit(values: Record<string, unknown>) {
  const casted = exportSchema.cast(values, {
    stripUnknown: true,
  }) as Omit<StartExportPayload, "outputDir">;
  const payload: StartExportPayload = {
    domain: String(casted.domain ?? "").trim(),
    outputDir: "",
    split: Boolean(casted.split),
    timestampFormat:
      String(casted.timestampFormat ?? defaultTimestampFormat).trim() ||
      defaultTimestampFormat,
    exportImages: Boolean(casted.exportImages),
  };

  emit("start", payload);
}
</script>

<template>
  <AppForm
    :schema="exportSchema"
    :initial-values="initialValues"
    title="Start Export"
    description="Sign in to Mi Cloud in the popup window and let the exporter scrape your notes. Export path comes from Settings."
    submit-label="Start Export"
    submitting-label="Starting..."
    @submit="handleSubmit"
  >
    <template #default="{ values }">
      <AppFormInputField
        name="domain"
        label="Domain"
        placeholder="us.i.mi.com"
        autocomplete="off"
      />

      <AppFormSwitchField
        name="split"
        label="Split notes into files"
        description="Create a separate markdown file per note instead of one aggregated file."
      />

      <AppFormInputField
        v-if="Boolean(values.split)"
        name="timestampFormat"
        label="Timestamp format"
        placeholder="dd-MM-yyyy_HH-mm-ss"
        description=".NET-style timestamp format used in split file names."
      />

      <AppFormSwitchField
        name="exportImages"
        label="Export embedded images"
        description="Save image attachments and add markdown links to them."
      />
    </template>
  </AppForm>
</template>
