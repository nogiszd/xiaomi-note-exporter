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
import { Input } from "@/components/ui/input";

const props = withDefaults(
  defineProps<{
    name: string;
    label: string;
    placeholder?: string;
    type?: string;
    description?: string;
    autocomplete?: string;
    disabled?: boolean;
  }>(),
  {
    placeholder: "",
    type: "text",
    description: "",
    autocomplete: "off",
    disabled: false,
  },
);

const { value, errorMessage, handleBlur } = useField<string>(() => props.name);
const fieldId = computed(() => `field-${props.name.replace(/[^a-zA-Z0-9_-]/g, "-")}`);
</script>

<template>
  <Field>
    <FieldLabel :for="fieldId">{{ label }}</FieldLabel>
    <FieldContent>
      <Input
        :id="fieldId"
        :model-value="value"
        :placeholder="placeholder"
        :type="type"
        :autocomplete="autocomplete"
        :disabled="disabled"
        @update:model-value="value = String($event)"
        @blur="handleBlur"
      />
      <FieldDescription v-if="description">{{ description }}</FieldDescription>
      <FieldError :errors="errorMessage ? [errorMessage] : []" />
    </FieldContent>
  </Field>
</template>
