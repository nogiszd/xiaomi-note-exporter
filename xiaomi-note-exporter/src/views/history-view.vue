<script setup lang="ts">
import { onMounted } from "vue";
import { Button } from "@/components/ui/button";
import { Card, CardContent } from "@/components/ui/card";
import SessionList from "@/components/history/session-list.vue";
import { useSessionsStore } from "@/stores/sessions";

const sessionsStore = useSessionsStore();

onMounted(() => {
  void sessionsStore.refresh();
});
</script>

<template>
  <section class="grid gap-4">
    <div class="flex justify-end">
      <Button variant="outline" type="button" :disabled="sessionsStore.loading" @click="sessionsStore.refresh()">
        {{ sessionsStore.loading ? "Refreshing..." : "Refresh" }}
      </Button>
    </div>

    <Card v-if="sessionsStore.error" class="border-destructive/50">
      <CardContent class="pt-6 text-sm text-destructive">
        {{ sessionsStore.error }}
      </CardContent>
    </Card>

    <SessionList :sessions="sessionsStore.items" />
  </section>
</template>
