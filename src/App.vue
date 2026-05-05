<script setup lang="ts">
import AppSidebar from "@/components/layout/app-sidebar.vue";
import AppHeader from "@/components/layout/app-header.vue";
import UpdateAvailableDialog from "@/components/update/update-available-dialog.vue";
import { SidebarProvider } from "@/components/ui/sidebar";

const exportStore = useExportStore();
const updateStore = useUpdateStore();

onMounted(() => {
  void exportStore.initializeListeners();
  void updateStore.checkUpdatesOnLaunch();
});
</script>

<template>
  <SidebarProvider class="h-svh">
    <AppSidebar />
    <div class="flex min-h-0 min-w-0 flex-1 flex-col">
      <AppHeader />
      <main class="flex min-h-0 flex-1 flex-col overflow-auto p-4 md:p-6">
        <RouterView />
      </main>
    </div>
    <UpdateAvailableDialog />
  </SidebarProvider>
</template>
