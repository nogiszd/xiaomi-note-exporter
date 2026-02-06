<script setup lang="ts">
import { computed } from "vue";
import { Card, CardContent } from "@/components/ui/card";
import { ScrollArea } from "@/components/ui/scroll-area";

const props = defineProps<{
  content: string;
}>();

function escapeHtml(value: string): string {
  return value
    .replace(/&/g, "&amp;")
    .replace(/</g, "&lt;")
    .replace(/>/g, "&gt;")
    .replace(/\"/g, "&quot;")
    .replace(/'/g, "&#39;");
}

const rendered = computed(() => {
  const escaped = escapeHtml(props.content);
  return escaped.replace(/\n/g, "<br/>");
});
</script>

<template>
  <Card>
    <CardContent class="pt-6">
      <ScrollArea class="h-[500px] rounded-md border p-4">
        <article class="text-sm leading-6" v-html="rendered" />
      </ScrollArea>
    </CardContent>
  </Card>
</template>
