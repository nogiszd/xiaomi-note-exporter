import { createRouter, createWebHashHistory } from "vue-router";
import ExportView from "@/views/export-view.vue";
import HistoryView from "@/views/history-view.vue";
import ConverterView from "@/views/converter-view.vue";
import ViewerView from "@/views/viewer-view.vue";
import SettingsView from "@/views/settings-view.vue";

const router = createRouter({
  history: createWebHashHistory(),
  routes: [
    {
      path: "/",
      redirect: "/export",
    },
    {
      path: "/export",
      name: "export",
      component: ExportView,
    },
    {
      path: "/history",
      name: "history",
      component: HistoryView,
    },
    {
      path: "/converter",
      name: "converter",
      component: ConverterView,
    },
    {
      path: "/viewer/:sessionId?",
      name: "viewer",
      component: ViewerView,
      props: true,
    },
    {
      path: "/settings",
      name: "settings",
      component: SettingsView,
    },
  ],
});

export default router;
