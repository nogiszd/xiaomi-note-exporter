<script setup lang="ts">
import { onMounted } from "vue";
import { RouterView } from "vue-router";
import AppSidebar from "@/components/layout/app-sidebar.vue";
import AppHeader from "@/components/layout/app-header.vue";
import { SidebarProvider } from "@/components/ui/sidebar";
import { useExportStore } from "@/stores/export";

const exportStore = useExportStore();

onMounted(() => {
  void exportStore.initializeListeners();
});
</script>

<template>
  <SidebarProvider>
    <AppSidebar />
    <div class="flex min-w-0 max-h-full flex-1 flex-col">
      <AppHeader />
      <main class="flex-1 p-4 md:p-6">
        <RouterView />
      </main>
    </div>
  </SidebarProvider>
</template>
