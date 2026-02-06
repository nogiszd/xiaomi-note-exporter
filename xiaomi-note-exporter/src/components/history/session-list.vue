<script setup lang="ts">
import { reactive } from "vue";
import { useRouter } from "vue-router";
import { openInExplorer } from "@/lib/api";
import { useSessionsStore } from "@/stores/sessions";
import type { Session } from "@/types";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import {
  AlertDialog,
  AlertDialogAction,
  AlertDialogCancel,
  AlertDialogContent,
  AlertDialogDescription,
  AlertDialogFooter,
  AlertDialogHeader,
  AlertDialogTitle,
  AlertDialogTrigger,
} from "@/components/ui/alert-dialog";
import { Switch } from "@/components/ui/switch";
import { Label } from "@/components/ui/label";
import {
  Table,
  TableBody,
  TableCell,
  TableEmpty,
  TableHead,
  TableHeader,
  TableRow,
} from "@/components/ui/table";

defineProps<{
  sessions: Session[];
}>();

const deleteFilesBySession = reactive<Record<string, boolean>>({});
const sessionsStore = useSessionsStore();
const router = useRouter();

function viewSession(sessionId: string) {
  void router.push(`/viewer/${sessionId}`);
}

async function openFolder(path: string) {
  await openInExplorer(path);
}

function formatDate(value: string) {
  const parsed = new Date(value);
  if (Number.isNaN(parsed.getTime())) {
    return value;
  }
  return parsed.toLocaleString();
}

function statusVariant(status: Session["status"]) {
  if (status === "completed") {
    return "secondary" as const;
  }
  if (status === "running") {
    return "default" as const;
  }
  if (status === "error") {
    return "destructive" as const;
  }
  return "outline" as const;
}

async function removeSession(sessionId: string) {
  const deleteFiles = deleteFilesBySession[sessionId] ?? false;
  await sessionsStore.remove(sessionId, deleteFiles);
  deleteFilesBySession[sessionId] = false;
}
</script>

<template>
  <section class="rounded-lg border">
    <Table>
      <TableHeader>
        <TableRow>
          <TableHead>Date</TableHead>
          <TableHead>Domain</TableHead>
          <TableHead>Notes</TableHead>
          <TableHead>Images</TableHead>
          <TableHead>Split</TableHead>
          <TableHead>Status</TableHead>
          <TableHead>Output</TableHead>
          <TableHead class="w-65">Actions</TableHead>
        </TableRow>
      </TableHeader>
      <TableBody>
        <template v-if="sessions.length > 0">
          <TableRow v-for="session in sessions" :key="session.id">
            <TableCell>{{ formatDate(session.startedAt) }}</TableCell>
            <TableCell>{{ session.domain }}</TableCell>
            <TableCell>{{ session.notesCount }}</TableCell>
            <TableCell>{{ session.imagesCount }}</TableCell>
            <TableCell>{{ session.splitMode ? "Yes" : "No" }}</TableCell>
            <TableCell>
              <Badge :variant="statusVariant(session.status)">{{
                session.status
              }}</Badge>
            </TableCell>
            <TableCell
              class="max-w-[320px] truncate"
              :title="session.outputPath"
              >{{ session.outputPath }}</TableCell
            >
            <TableCell>
              <div class="flex flex-wrap gap-2">
                <Button
                  size="sm"
                  variant="outline"
                  type="button"
                  @click="viewSession(session.id)"
                  >View</Button
                >
                <Button
                  size="sm"
                  variant="outline"
                  type="button"
                  @click="openFolder(session.outputPath)"
                  >Open</Button
                >

                <AlertDialog>
                  <AlertDialogTrigger as-child>
                    <Button size="sm" variant="destructive" type="button"
                      >Delete</Button
                    >
                  </AlertDialogTrigger>
                  <AlertDialogContent>
                    <AlertDialogHeader>
                      <AlertDialogTitle
                        >Delete export session?</AlertDialogTitle
                      >
                      <AlertDialogDescription>
                        This removes the session from history. Optionally remove
                        exported files as well.
                      </AlertDialogDescription>
                    </AlertDialogHeader>

                    <div
                      class="flex items-center justify-between gap-3 rounded-md border p-3"
                    >
                      <Label :for="`delete-files-${session.id}`"
                        >Delete exported files too</Label
                      >
                      <Switch
                        :id="`delete-files-${session.id}`"
                        :model-value="deleteFilesBySession[session.id] ?? false"
                        @update:model-value="
                          deleteFilesBySession[session.id] = Boolean($event)
                        "
                      />
                    </div>

                    <AlertDialogFooter>
                      <AlertDialogCancel>Cancel</AlertDialogCancel>
                      <AlertDialogAction @click="removeSession(session.id)"
                        >Delete</AlertDialogAction
                      >
                    </AlertDialogFooter>
                  </AlertDialogContent>
                </AlertDialog>
              </div>
            </TableCell>
          </TableRow>
        </template>

        <TableEmpty v-else :colspan="8">No export sessions found.</TableEmpty>
      </TableBody>
    </Table>
  </section>
</template>
