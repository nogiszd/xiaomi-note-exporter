<script setup lang="ts">
import { Button } from "@/components/ui/button";
import { Card, CardContent, CardFooter } from "@/components/ui/card";
import { Textarea } from "@/components/ui/textarea";

const props = defineProps<{
  modelValue: string;
}>();

const emit = defineEmits<{
  "update:modelValue": [value: string];
  save: [];
}>();
</script>

<template>
  <Card>
    <CardContent class="pt-6">
      <Textarea
        class="min-h-105 font-mono text-sm"
        :value="props.modelValue"
        @input="
          emit(
            'update:modelValue',
            ($event.target as HTMLTextAreaElement).value,
          )
        "
      />
    </CardContent>
    <CardFooter>
      <Button type="button" @click="emit('save')">Save</Button>
    </CardFooter>
  </Card>
</template>
