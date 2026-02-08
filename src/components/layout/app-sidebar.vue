<script setup lang="ts">
import { computed, onMounted, ref, watch } from "vue";
import { useRoute, useRouter } from "vue-router";
import {
  Activity,
  Ban,
  CheckCircle2,
  CircleHelp,
  CircleX,
  Download,
  FileJson2,
  History,
  LoaderCircle,
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
import AppAbout from "@/components/layout/app-about.vue";
import logo from "@/assets/logo.png";

const router = useRouter();
const route = useRoute();
const exportStore = useExportStore();
const { state, open, setOpen, isMobile } = useSidebar();
const SIDEBAR_STORAGE_KEY = "xne.sidebar.open";
const isAboutOpen = ref(false);

type SecondaryNavItem =
  | {
      type: "route";
      to: string;
      label: string;
      icon: unknown;
    }
  | {
      type: "action";
      label: string;
      icon: unknown;
      onClick: () => void;
    };

const primaryNavItems = [
  { to: "/export", label: "Export", icon: Download },
  { to: "/history", label: "History", icon: History },
  { to: "/converter", label: "Converter", icon: FileJson2 },
];

const secondaryNavItems: SecondaryNavItem[] = [
  { type: "route", to: "/settings", label: "Settings", icon: Settings },
  {
    type: "action",
    label: "About",
    icon: CircleHelp,
    onClick: () => {
      isAboutOpen.value = true;
    },
  },
];

const isCollapsed = computed(() => state.value === "collapsed");
const statusLabel = computed(() => exportStore.statusText || "Idle");
const statusMeta = computed(() => {
  const normalized = statusLabel.value.toLowerCase();

  if (
    exportStore.isRunning ||
    normalized.startsWith("starting") ||
    normalized.startsWith("exporting")
  ) {
    return {
      icon: LoaderCircle,
      iconClass: "size-4! animate-spin text-sky-500",
    };
  }

  if (normalized.startsWith("completed")) {
    return { icon: CheckCircle2, iconClass: "size-4! text-emerald-500" };
  }

  if (normalized.startsWith("cancel")) {
    return { icon: Ban, iconClass: "size-4! text-amber-500" };
  }

  if (normalized.startsWith("error")) {
    return { icon: CircleX, iconClass: "size-4! text-destructive" };
  }

  return { icon: Play, iconClass: "size-4! text-muted-foreground" };
});

function isActive(path: string) {
  return route.path.startsWith(path);
}

function goToExport() {
  void router.push("/export");
}

function readStoredSidebarOpen(): boolean | null {
  try {
    const raw = window.localStorage.getItem(SIDEBAR_STORAGE_KEY);
    if (raw === "true") {
      return true;
    }
    if (raw === "false") {
      return false;
    }
    return null;
  } catch {
    return null;
  }
}

function persistSidebarOpen(value: boolean) {
  try {
    window.localStorage.setItem(SIDEBAR_STORAGE_KEY, String(value));
  } catch {}
}

onMounted(() => {
  if (isMobile.value) {
    return;
  }

  const stored = readStoredSidebarOpen();
  if (stored === null) {
    persistSidebarOpen(open.value);
    return;
  }

  setOpen(stored);
});

watch(open, (value) => {
  if (isMobile.value) {
    return;
  }
  persistSidebarOpen(value);
});
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
            <SidebarMenuItem v-for="item in primaryNavItems" :key="item.to">
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
            <SidebarMenuItem
              v-for="item in secondaryNavItems"
              :key="item.type === 'route' ? item.to : item.label"
            >
              <SidebarMenuButton
                v-if="item.type === 'route'"
                as-child
                :is-active="isActive(item.to)"
                :tooltip="item.label"
              >
                <RouterLink :to="item.to" class="flex items-center gap-2">
                  <component :is="item.icon" />
                  <span>{{ item.label }}</span>
                </RouterLink>
              </SidebarMenuButton>
              <SidebarMenuButton
                v-else
                :tooltip="item.label"
                @click="item.onClick"
              >
                <component :is="item.icon" />
                <span>{{ item.label }}</span>
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
        class="w-full gap-1 text-sm group-data-[collapsible=icon]:mx-auto group-data-[collapsible=icon]:px-1"
      >
        <component :is="statusMeta.icon" :class="statusMeta.iconClass" />
        <span class="group-data-[collapsible=icon]:hidden">{{
          statusLabel
        }}</span>
      </Badge>
    </SidebarFooter>
  </Sidebar>

  <AppAbout v-model:open="isAboutOpen" />
</template>
