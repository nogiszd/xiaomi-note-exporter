import { createApp } from "vue";
import { createPinia } from "pinia";
import App from "./App.vue";
import router from "./router";
import { useSettingsStore } from "@/stores/settings";
import "./style.css";

async function bootstrap() {
  const app = createApp(App);
  const pinia = createPinia();

  app.use(pinia);
  app.use(router);

  const settingsStore = useSettingsStore(pinia);
  await settingsStore.load();

  await router.isReady();
  app.mount("#app");
}

void bootstrap();
