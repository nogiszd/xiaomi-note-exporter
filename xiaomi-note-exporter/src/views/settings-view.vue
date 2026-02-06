<script setup lang="ts">
import { computed, onMounted, ref } from "vue";
import * as yup from "yup";
import AppForm from "@/components/forms/app-form.vue";
import AppFormPathField from "@/components/forms/app-form-path-field.vue";
import AppFormSwitchField from "@/components/forms/app-form-switch-field.vue";
import { Button } from "@/components/ui/button";
import { Card, CardContent } from "@/components/ui/card";
import { openInExplorer } from "@/lib/api";
import { useSettingsStore } from "@/stores/settings";
import type { AppSettings } from "@/types";

const settingsStore = useSettingsStore();
const saveMessage = ref("");

const settingsSchema = yup.object({
  defaultExportDir: yup
    .string()
    .trim()
    .required("Default export directory is required."),
  darkMode: yup.boolean().required(),
});

const formKey = computed(
  () =>
    `${settingsStore.settings.defaultExportDir}|${settingsStore.settings.darkMode ? "1" : "0"}`,
);

const initialValues = computed<AppSettings>(() => ({
  defaultExportDir: settingsStore.settings.defaultExportDir,
  darkMode: settingsStore.settings.darkMode,
}));

async function saveSettings(values: Record<string, unknown>) {
  const casted = settingsSchema.cast(values, {
    stripUnknown: true,
  }) as AppSettings;
  await settingsStore.save({
    defaultExportDir: casted.defaultExportDir.trim(),
    darkMode: Boolean(casted.darkMode),
  });
  saveMessage.value = "Settings saved.";
}

async function openDefaultDirectory() {
  if (!settingsStore.settings.defaultExportDir) {
    return;
  }
  await openInExplorer(settingsStore.settings.defaultExportDir);
}

onMounted(() => {
  if (!settingsStore.loaded) {
    void settingsStore.load();
  }
});
</script>

<template>
  <section class="grid gap-6">
    <Card v-if="!settingsStore.loaded || settingsStore.loading">
      <CardContent class="pt-6 text-sm text-muted-foreground"
        >Loading settings...</CardContent
      >
    </Card>

    <AppForm
      v-else
      :key="formKey"
      :schema="settingsSchema"
      :initial-values="initialValues"
      title="Settings"
      description="Configure default app settings."
      submit-label="Save Settings"
      submitting-label="Saving..."
      @submit="saveSettings"
    >
      <template #default>
        <AppFormPathField
          name="defaultExportDir"
          label="Default export directory"
          mode="directory"
          dialog-title="Select default export directory"
          placeholder="C:\\Users\\you\\Documents\\Xiaomi Note Exporter"
          description="This folder is created on app startup and used by default in Export."
        />

        <AppFormSwitchField
          name="darkMode"
          label="Dark mode"
          description="Toggle application appearance."
        />

        <p v-if="settingsStore.error" class="text-xs text-destructive">
          {{ settingsStore.error }}
        </p>
        <p v-if="saveMessage" class="text-xs text-muted-foreground">
          {{ saveMessage }}
        </p>
      </template>

      <template #footer>
        <div class="flex w-full items-center justify-between gap-3">
          <Button type="button" variant="outline" @click="openDefaultDirectory">
            Open Default Directory
          </Button>
          <Button type="submit" :disabled="settingsStore.loading"
            >Save Settings</Button
          >
        </div>
      </template>
    </AppForm>
  </section>
</template>
