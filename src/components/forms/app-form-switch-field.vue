<script setup lang="ts">
import { computed } from "vue";
import { useField } from "vee-validate";
import {
  Field,
  FieldContent,
  FieldDescription,
  FieldError,
  FieldLabel,
} from "@/components/ui/field";
import { Switch } from "@/components/ui/switch";

const props = withDefaults(
  defineProps<{
    name: string;
    label: string;
    description?: string;
    disabled?: boolean;
  }>(),
  {
    description: "",
    disabled: false,
  },
);

const { value, errorMessage, setValue } = useField<boolean>(() => props.name);
const fieldId = computed(() => `field-${props.name.replace(/[^a-zA-Z0-9_-]/g, "-")}`);
</script>

<template>
  <Field orientation="horizontal" class="items-center rounded-lg border p-3">
    <FieldContent>
      <FieldLabel :for="fieldId">{{ label }}</FieldLabel>
      <FieldDescription v-if="description">{{ description }}</FieldDescription>
      <FieldError :errors="errorMessage ? [errorMessage] : []" />
    </FieldContent>
    <div class="flex items-center">
      <Switch
        :id="fieldId"
        :disabled="disabled"
        :model-value="Boolean(value)"
        @update:model-value="setValue(Boolean($event))"
      />
    </div>
  </Field>
</template>
