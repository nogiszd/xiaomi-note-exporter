import { createApp } from "vue";
import { createPinia } from "pinia";
import App from "./App.vue";
import router from "./router";
import { useSettingsStore } from "@/stores/settings";
import { closeSplashscreen } from "@/lib/api";
import "./style.css";

const SPLASH_DELAY_MS = import.meta.env.DEV ? 10_000 : 2_000;

function sleep(milliseconds: number): Promise<void> {
  return new Promise((resolve) => {
    window.setTimeout(resolve, milliseconds);
  });
}

async function bootstrap() {
  const app = createApp(App);
  const pinia = createPinia();

  try {
    app.use(pinia);
    app.use(router);

    const settingsStore = useSettingsStore(pinia);
    await settingsStore.load();

    await router.isReady();
    app.mount("#app");
  } catch (error) {
    console.error("Failed to bootstrap app", error);
  } finally {
    await sleep(SPLASH_DELAY_MS);

    try {
      await closeSplashscreen();
    } catch (error) {
      console.error("Failed to close splashscreen", error);
    }
  }
}

void bootstrap();
