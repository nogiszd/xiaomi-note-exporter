<script setup lang="ts">
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
    borderless?: boolean;
  }>(),
  {
    description: "",
    disabled: false,
    borderless: false,
  },
);

const { value, errorMessage, setValue } = useField<boolean>(() => props.name);
const fieldId = computed(() => `field-${props.name.replace(/[^a-zA-Z0-9_-]/g, "-")}`);
const wrapperClass = computed(() =>
  props.borderless ? "items-center" : "items-center rounded-lg border p-3",
);
</script>

<template>
  <Field orientation="horizontal" :class="wrapperClass">
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
