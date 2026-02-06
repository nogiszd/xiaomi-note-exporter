import { defineStore } from "pinia";
import { deleteSession, getSession, getSessions } from "@/lib/api";
import type { Session } from "@/types";

interface SessionsState {
  items: Session[];
  loading: boolean;
  error: string;
}

export const useSessionsStore = defineStore("sessions", {
  state: (): SessionsState => ({
    items: [],
    loading: false,
    error: "",
  }),
  actions: {
    async refresh() {
      this.loading = true;
      this.error = "";
      try {
        this.items = await getSessions(1, 200);
      } catch (error) {
        this.error = error instanceof Error ? error.message : "Failed to load sessions.";
      } finally {
        this.loading = false;
      }
    },
    async findById(id: string): Promise<Session | null> {
      return getSession(id);
    },
    async remove(id: string, deleteFiles: boolean) {
      await deleteSession(id, deleteFiles);
      this.items = this.items.filter((session) => session.id !== id);
    },
  },
});
