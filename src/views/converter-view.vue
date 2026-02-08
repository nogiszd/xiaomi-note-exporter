<script setup lang="ts">
import { ref } from "vue";
import ConverterForm from "@/components/converter/converter-form.vue";
import { convertToJson, openInExplorer } from "@/lib/api";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import {
  Card,
  CardContent,
  CardFooter,
  CardHeader,
  CardTitle,
} from "@/components/ui/card";
import CardAction from "@/components/ui/card/CardAction.vue";

const busy = ref(false);
const resultPath = ref("");
const errorMessage = ref("");

async function handleConvert(sourcePath: string, outputPath: string) {
  busy.value = true;
  resultPath.value = "";
  errorMessage.value = "";
  try {
    resultPath.value = await convertToJson(sourcePath, outputPath);
  } catch (error) {
    errorMessage.value =
      error instanceof Error ? error.message : "Conversion failed.";
  } finally {
    busy.value = false;
  }
}

async function openResult() {
  if (!resultPath.value) {
    return;
  }
  await openInExplorer(resultPath.value);
}
</script>

<template>
  <section class="grid gap-6">
    <ConverterForm @convert="handleConvert" />

    <Card v-if="busy">
      <CardContent class="pt-6 text-sm text-muted-foreground"
        >Converting...</CardContent
      >
    </Card>

    <Card v-if="errorMessage" class="border-destructive/50">
      <CardContent class="pt-6 text-sm text-destructive">{{
        errorMessage
      }}</CardContent>
    </Card>

    <Card v-if="resultPath">
      <CardHeader class="flex-row items-center justify-between">
        <CardTitle class="text-base">JSON created</CardTitle>
        <CardAction>
          <Badge variant="secondary">Success</Badge>
        </CardAction>
      </CardHeader>
      <CardContent class="text-sm text-muted-foreground">{{
        resultPath
      }}</CardContent>
      <CardFooter>
        <Button type="button" variant="outline" @click="openResult"
          >Open File</Button
        >
      </CardFooter>
    </Card>
  </section>
</template>
