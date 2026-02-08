<script setup lang="ts">
import type { AnyObjectSchema } from "yup";
import { Button } from "@/components/ui/button";
import {
  Card,
  CardContent,
  CardDescription,
  CardFooter,
  CardHeader,
  CardTitle,
} from "@/components/ui/card";
import { useYupForm } from "@/composables/use-yup-form";

const props = withDefaults(
  defineProps<{
    schema: AnyObjectSchema;
    initialValues: object;
    title: string;
    description?: string;
    submitLabel?: string;
    submittingLabel?: string;
    submitDisabled?: boolean;
  }>(),
  {
    description: "",
    submitLabel: "Submit",
    submittingLabel: "Submitting...",
    submitDisabled: false,
  },
);

const emit = defineEmits<{
  submit: [values: Record<string, unknown>];
}>();

const {
  handleSubmit,
  values,
  errors,
  isSubmitting,
  setFieldValue,
  setFieldError,
  resetForm,
} = useYupForm(props.schema, props.initialValues as Record<string, unknown>);

const onSubmit = handleSubmit(async (formValues) => {
  emit("submit", formValues as Record<string, unknown>);
});
</script>

<template>
  <form novalidate @submit="onSubmit">
    <Card>
      <CardHeader>
        <CardTitle>{{ title }}</CardTitle>
        <CardDescription v-if="description">{{ description }}</CardDescription>
      </CardHeader>
      <CardContent class="grid gap-5">
        <slot
          :values="values"
          :errors="errors"
          :is-submitting="isSubmitting"
          :set-field-value="setFieldValue"
          :set-field-error="setFieldError"
          :reset-form="resetForm"
        />
      </CardContent>
      <CardFooter class="flex items-center justify-between gap-3">
        <slot name="footer" :is-submitting="isSubmitting">
          <Button type="submit" :disabled="submitDisabled || isSubmitting">
            {{ isSubmitting ? submittingLabel : submitLabel }}
          </Button>
        </slot>
      </CardFooter>
    </Card>
  </form>
</template>
