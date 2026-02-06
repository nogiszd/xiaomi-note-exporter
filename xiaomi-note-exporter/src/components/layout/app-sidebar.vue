<script setup lang="ts">
import { computed } from "vue";
import { useRoute, useRouter } from "vue-router";
import {
  Activity,
  Download,
  FileJson2,
  History,
  Play,
  Settings,
} from "lucide-vue-next";
import { Badge } from "@/components/ui/badge";
import {
  Sidebar,
  SidebarContent,
  SidebarFooter,
  SidebarGroup,
  SidebarGroupContent,
  SidebarHeader,
  SidebarMenu,
  SidebarMenuButton,
  SidebarMenuItem,
  SidebarSeparator,
  useSidebar,
} from "@/components/ui/sidebar";
import { useExportStore } from "@/stores/export";
import logo from "@/assets/logo.png";

const router = useRouter();
const route = useRoute();
const exportStore = useExportStore();
const { state } = useSidebar();

const navItems = [
  { to: "/export", label: "Export", icon: Download },
  { to: "/history", label: "History", icon: History },
  { to: "/converter", label: "Converter", icon: FileJson2 },
];

const isCollapsed = computed(() => state.value === "collapsed");
const statusLabel = computed(() => exportStore.statusText || "Idle");

function isActive(path: string) {
  return route.path.startsWith(path);
}

function goToExport() {
  void router.push("/export");
}
</script>

<template>
  <Sidebar collapsible="icon" variant="sidebar">
    <SidebarHeader>
      <SidebarMenu>
        <SidebarMenuItem>
          <SidebarMenuButton
            size="lg"
            :tooltip="isCollapsed ? 'Xiaomi Notes Exporter' : undefined"
          >
            <div
              class="flex aspect-square size-8 items-center justify-center rounded-lg overflow-clip text-primary-foreground"
            >
              <img :src="logo" />
            </div>
            <div
              class="grid flex-1 text-left text-sm leading-tight group-data-[collapsible=icon]:hidden"
            >
              <span class="truncate font-semibold">Xiaomi Notes Exporter</span>
            </div>
          </SidebarMenuButton>
        </SidebarMenuItem>
      </SidebarMenu>
    </SidebarHeader>

    <SidebarContent>
      <SidebarGroup>
        <SidebarGroupContent>
          <SidebarMenu>
            <SidebarMenuItem v-for="item in navItems" :key="item.to">
              <SidebarMenuButton
                as-child
                :is-active="isActive(item.to)"
                :tooltip="item.label"
              >
                <RouterLink :to="item.to" class="flex items-center gap-2">
                  <component :is="item.icon" />
                  <span>{{ item.label }}</span>
                </RouterLink>
              </SidebarMenuButton>
            </SidebarMenuItem>
          </SidebarMenu>
        </SidebarGroupContent>
      </SidebarGroup>

      <SidebarSeparator />

      <SidebarGroup>
        <SidebarGroupContent>
          <SidebarMenu>
            <SidebarMenuItem>
              <SidebarMenuButton
                as-child
                :is-active="isActive('/settings')"
                tooltip="Settings"
              >
                <RouterLink to="/settings" class="flex items-center gap-2">
                  <component :is="Settings" />
                  <span>Settings</span>
                </RouterLink>
              </SidebarMenuButton>
            </SidebarMenuItem>
          </SidebarMenu>
        </SidebarGroupContent>
      </SidebarGroup>
    </SidebarContent>

    <SidebarFooter>
      <SidebarMenu>
        <SidebarMenuItem>
          <SidebarMenuButton :tooltip="statusLabel" @click="goToExport">
            <Activity />
            <span>{{
              exportStore.isRunning ? "View Running Export" : "Go to Export"
            }}</span>
          </SidebarMenuButton>
        </SidebarMenuItem>
      </SidebarMenu>
      <Badge
        variant="outline"
        class="w-full gap-1 group-data-[collapsible=icon]:mx-auto group-data-[collapsible=icon]:px-1"
      >
        <Play class="size-3" />
        <span class="group-data-[collapsible=icon]:hidden">{{
          statusLabel
        }}</span>
      </Badge>
    </SidebarFooter>
  </Sidebar>
</template>
