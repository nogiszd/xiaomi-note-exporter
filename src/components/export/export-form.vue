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
const defaultCreatedDateFormat = "dd/MM/yyyy HH:mm";

const exportSchema = yup.object({
  domain: yup
    .string()
    .trim()
    .required("Domain is required.")
    .matches(
      /^([a-z0-9-]+\.)+[a-z]{2,}$/i,
      "Enter a valid domain like us.i.mi.com.",
    ),
  split: yup.boolean().required(),
  nameByTitle: yup.boolean().required(),
  timestampFormat: yup.string().when(["split", "nameByTitle"], {
    is: (split: boolean, nameByTitle: boolean) => split && !nameByTitle,
    then: (schema) =>
      schema
        .trim()
        .required("Timestamp format is required when naming files by date."),
    otherwise: (schema) => schema.default(defaultTimestampFormat),
  }),
  createdDateFormat: yup
    .string()
    .trim()
    .required("Created date format is required.")
    .default(defaultCreatedDateFormat),
  exportImages: yup.boolean().required(),
});

const initialValues = {
  domain: "us.i.mi.com",
  split: false,
  nameByTitle: false,
  timestampFormat: defaultTimestampFormat,
  createdDateFormat: defaultCreatedDateFormat,
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
    nameByTitle: Boolean(casted.nameByTitle),
    timestampFormat:
      String(casted.timestampFormat ?? defaultTimestampFormat).trim() ||
      defaultTimestampFormat,
    createdDateFormat:
      String(casted.createdDateFormat ?? defaultCreatedDateFormat).trim() ||
      defaultCreatedDateFormat,
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

      <div class="grid gap-4 rounded-lg border p-3">
        <AppFormSwitchField
          name="split"
          label="Split notes into files"
          description="Create a separate markdown file per note instead of one aggregated file."
          borderless
        />

        <template v-if="Boolean(values.split)">
          <AppFormSwitchField
            name="nameByTitle"
            label="Name files by note title"
            description="When on, each file is named after its note title (falls back to the timestamp format if the title is empty)."
            borderless
          />

          <AppFormInputField
            v-if="!Boolean(values.nameByTitle)"
            name="timestampFormat"
            label="Timestamp format"
            placeholder="dd-MM-yyyy_HH-mm-ss"
            description=".NET-style timestamp format used in split file names."
          />
        </template>
      </div>

      <div class="grid rounded-lg border p-3">
        <AppFormInputField
          name="createdDateFormat"
          label="Created date format"
          placeholder="dd/MM/yyyy HH:mm"
          description=".NET-style creation date format appended to each note (e.g. Created at: 05/05/2026 13:42)."
        />
      </div>

      <AppFormSwitchField
        name="exportImages"
        label="Export embedded images"
        description="Save image attachments and add markdown links to them."
      />
    </template>
  </AppForm>
</template>
