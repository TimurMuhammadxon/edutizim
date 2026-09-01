import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import { useEffect, useState } from "react";
import { tasksApi } from "@/api/tasks";
import { leadsApi } from "@/api/leads";
import { useAuthStore } from "@/store/auth";
import { Card, CardContent } from "@/components/ui/card";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Textarea } from "@/components/ui/textarea";
import { Badge } from "@/components/ui/badge";
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "@/components/ui/select";
import { PageLoader } from "@/components/shared/LoadingSpinner";
import { CrudFormDialog } from "@/components/shared/CrudFormDialog";
import { toast } from "@/components/ui/use-toast";
import { getApiErrorMessage } from "@/lib/errors";
import { useTranslation } from "@/lib/i18n";
import { Plus, Check, X } from "lucide-react";
import { cn } from "@/lib/utils";

type FormState = { title: string; description: string; dueAt: string; leadId: string };
const emptyForm: FormState = { title: "", description: "", dueAt: "", leadId: "" };

export function TasksPage() {
  const t = useTranslation();
  const qc = useQueryClient();
  const user = useAuthStore((s) => s.user);
  const [dialogOpen, setDialogOpen] = useState(false);
  const [form, setForm] = useState<FormState>(emptyForm);

  const { data, isLoading } = useQuery({
    queryKey: ["crm-tasks"],
    queryFn: () => tasksApi.list({ status: "Pending", pageSize: 100 }),
  });

  const { data: leads } = useQuery({
    queryKey: ["crm-leads-all"],
    queryFn: () => leadsApi.list({ pageSize: 100 }),
  });

  const onError = (e: unknown) =>
    toast({ title: t.error, description: getApiErrorMessage(e), variant: "destructive" });

  const createMutation = useMutation({
    mutationFn: () =>
      tasksApi.create({
        title: form.title,
        description: form.description || undefined,
        dueAt: new Date(form.dueAt).toISOString(),
        assignedToUserId: user!.id,
        leadId: form.leadId || undefined,
      }),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ["crm-tasks"] });
      setDialogOpen(false);
      setForm(emptyForm);
      toast({ title: t.taskAdded });
    },
    onError,
  });

  const completeMutation = useMutation({
    mutationFn: (id: string) => tasksApi.complete(id),
    onSuccess: () => qc.invalidateQueries({ queryKey: ["crm-tasks"] }),
    onError,
  });

  const cancelMutation = useMutation({
    mutationFn: (id: string) => tasksApi.cancel(id),
    onSuccess: () => qc.invalidateQueries({ queryKey: ["crm-tasks"] }),
    onError,
  });

  // Date.now() must run in an effect, not during render (React purity rule) — until
  // it fires, `overdue` below just treats every task as not-yet-overdue for one tick.
  const [now, setNow] = useState<number | null>(null);
  useEffect(() => {
    Promise.resolve().then(() => setNow(Date.now()));
  }, [data]);

  if (isLoading) return <PageLoader />;

  return (
    <div className="max-w-3xl mx-auto space-y-6">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-2xl font-bold">{t.tasksTitle}</h1>
          <p className="text-muted-foreground mt-1">{t.tasksSubtitle}</p>
        </div>
        <Button onClick={() => setDialogOpen(true)}>
          <Plus className="h-4 w-4 mr-2" />
          {t.addTask}
        </Button>
      </div>

      <div className="space-y-3">
        {data?.items.map((task) => {
          const overdue = now !== null && new Date(task.dueAt).getTime() < now;
          return (
            <Card key={task.id}>
              <CardContent className="p-4 flex items-start justify-between gap-4">
                <div className="min-w-0">
                  <div className="flex items-center gap-2">
                    <p className="font-medium">{task.title}</p>
                    {overdue && <Badge variant="destructive" className="text-xs">{t.overdueBadge}</Badge>}
                  </div>
                  {task.leadFullName && (
                    <p className="text-sm text-muted-foreground mt-0.5">{task.leadFullName}</p>
                  )}
                  {task.description && (
                    <p className="text-sm text-muted-foreground mt-1">{task.description}</p>
                  )}
                  <p className={cn("text-xs mt-2", overdue ? "text-red-500 font-medium" : "text-muted-foreground")}>
                    {new Date(task.dueAt).toLocaleString("ru-RU")}
                  </p>
                </div>
                <div className="flex gap-2 flex-shrink-0">
                  <Button variant="outline" size="icon" onClick={() => completeMutation.mutate(task.id)} title={t.doneAction}>
                    <Check className="h-4 w-4 text-emerald-500" />
                  </Button>
                  <Button variant="outline" size="icon" onClick={() => cancelMutation.mutate(task.id)} title={t.cancel}>
                    <X className="h-4 w-4 text-red-500" />
                  </Button>
                </div>
              </CardContent>
            </Card>
          );
        })}
        {data?.items.length === 0 && (
          <Card>
            <CardContent className="text-center text-muted-foreground py-10">
              {t.noActiveTasks}
            </CardContent>
          </Card>
        )}
      </div>

      <CrudFormDialog
        open={dialogOpen}
        onOpenChange={setDialogOpen}
        title={t.newTask}
        onSave={() => createMutation.mutate()}
        saveDisabled={!form.title.trim() || !form.dueAt || createMutation.isPending}
        saving={createMutation.isPending}
      >
        <div className="space-y-1.5">
          <Label>{t.title}</Label>
          <Input value={form.title} onChange={(e) => setForm({ ...form, title: e.target.value })} />
        </div>
        <div className="space-y-1.5">
          <Label>{t.dueDateLabel}</Label>
          <Input type="datetime-local" value={form.dueAt} onChange={(e) => setForm({ ...form, dueAt: e.target.value })} />
        </div>
        <div className="space-y-1.5">
          <Label>{t.optionalLeadLabel}</Label>
          <Select value={form.leadId} onValueChange={(v) => setForm({ ...form, leadId: v })}>
            <SelectTrigger><SelectValue placeholder={t.selectLead} /></SelectTrigger>
            <SelectContent>
              {leads?.items.map((l) => (
                <SelectItem key={l.id} value={l.id}>{l.fullName} — {l.phone}</SelectItem>
              ))}
            </SelectContent>
          </Select>
        </div>
        <div className="space-y-1.5">
          <Label>{t.crmNotes}</Label>
          <Textarea value={form.description} onChange={(e) => setForm({ ...form, description: e.target.value })} />
        </div>
      </CrudFormDialog>
    </div>
  );
}
