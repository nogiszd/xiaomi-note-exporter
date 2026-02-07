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
import {
  Tooltip,
  TooltipContent,
  TooltipProvider,
  TooltipTrigger,
} from "@/components/ui/tooltip";
import { firstLetterToUpperCase } from "@/lib/utils";
import { Eye, FolderOpen, Trash2 } from "lucide-vue-next";

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

function statusStyle(status: Session["status"]) {
  if (status === "completed") {
    return "bg-success text-secondary dark:text-primary";
  }
  if (status === "running") {
    return "bg-sky-700 text-secondary dark:text-primary";
  }
  if (status === "error") {
    return "bg-destructive text-secondary dark:text-default";
  }
  return "bg-muted text-default";
}

async function removeSession(sessionId: string) {
  const deleteFiles = deleteFilesBySession[sessionId] ?? false;
  await sessionsStore.remove(sessionId, deleteFiles);
  deleteFilesBySession[sessionId] = false;
}
</script>

<template>
  <section class="rounded-lg border overflow-auto">
    <Table class="table-fixed">
      <TableHeader>
        <TableRow>
          <TableHead class="w-40">Date</TableHead>
          <TableHead class="w-32">Domain</TableHead>
          <TableHead class="w-20">Notes</TableHead>
          <TableHead class="w-20">Images</TableHead>
          <TableHead class="w-20">Split</TableHead>
          <TableHead class="w-28">Status</TableHead>
          <TableHead>Output</TableHead>
          <TableHead class="w-44 text-right">Actions</TableHead>
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
              <Badge variant="default" :class="statusStyle(session.status)">{{
                firstLetterToUpperCase(session.status)
              }}</Badge>
            </TableCell>
            <TableCell class="truncate" :title="session.outputPath">{{
              session.outputPath
            }}</TableCell>
            <TooltipProvider :disable-hoverable-content="true">
              <TableCell>
                <div class="flex flex-wrap gap-2 justify-end">
                  <Tooltip>
                    <TooltipTrigger as-child>
                      <Button
                        size="sm"
                        variant="outline"
                        type="button"
                        @click="viewSession(session.id)"
                      >
                        <Eye />
                      </Button>
                    </TooltipTrigger>
                    <TooltipContent>
                      <p>View session</p>
                    </TooltipContent>
                  </Tooltip>

                  <Tooltip>
                    <TooltipTrigger as-child>
                      <Button
                        size="sm"
                        variant="outline"
                        type="button"
                        @click="openFolder(session.outputPath)"
                      >
                        <FolderOpen />
                      </Button>
                    </TooltipTrigger>
                    <TooltipContent>
                      <p>Open output folder</p>
                    </TooltipContent>
                  </Tooltip>

                  <AlertDialog>
                    <AlertDialogTrigger as-child>
                      <Tooltip>
                        <TooltipTrigger as-child>
                          <Button size="sm" variant="destructive" type="button">
                            <Trash2 />
                          </Button>
                        </TooltipTrigger>
                        <TooltipContent>
                          <p>Delete session</p>
                        </TooltipContent>
                      </Tooltip>
                    </AlertDialogTrigger>
                    <AlertDialogContent>
                      <AlertDialogHeader>
                        <AlertDialogTitle
                          >Delete export session?</AlertDialogTitle
                        >
                        <AlertDialogDescription>
                          This removes the session from history. Optionally
                          remove exported files as well.
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
                          :model-value="
                            deleteFilesBySession[session.id] ?? false
                          "
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
            </TooltipProvider>
          </TableRow>
        </template>

        <TableEmpty v-else :colspan="8">No export sessions found.</TableEmpty>
      </TableBody>
    </Table>
  </section>
</template>
