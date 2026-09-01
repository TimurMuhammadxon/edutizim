import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import { useState } from "react";
import { useNavigate } from "react-router-dom";
import { groupsApi } from "@/api/groups";
import { useBranchStore } from "@/store/branch";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Textarea } from "@/components/ui/textarea";
import { Badge } from "@/components/ui/badge";
import { Switch } from "@/components/ui/switch";
import { PageLoader } from "@/components/shared/LoadingSpinner";
import { AttendanceDialog } from "@/components/shared/AttendanceDialog";
import { CrudTable, type CrudColumn } from "@/components/shared/CrudTable";
import { CrudFormDialog } from "@/components/shared/CrudFormDialog";
import { toast } from "@/components/ui/use-toast";
import { getApiErrorMessage } from "@/lib/errors";
import { useTranslation } from "@/lib/i18n";
import { Plus, Pencil, CalendarCheck } from "lucide-react";
import type { GroupDto } from "@/types";

type FormState = { name: string; price: string; description: string };
const emptyForm: FormState = { name: "", price: "", description: "" };

export function GroupsPage() {
  const t = useTranslation();
  const qc = useQueryClient();
  const navigate = useNavigate();
  const branchId = useBranchStore((s) => s.branchId);
  const [dialogOpen, setDialogOpen] = useState(false);
  const [editing, setEditing] = useState<GroupDto | null>(null);
  const [form, setForm] = useState<FormState>(emptyForm);
  const [attendanceGroup, setAttendanceGroup] = useState<GroupDto | null>(null);

  const { data, isLoading } = useQuery({
    queryKey: ["crm-groups", branchId],
    queryFn: () => groupsApi.list({ branchId: branchId ?? undefined, pageSize: 100 }),
  });

  const onError = (e: unknown) =>
    toast({ title: t.error, description: getApiErrorMessage(e), variant: "destructive" });

  const createMutation = useMutation({
    mutationFn: () =>
      groupsApi.create({ branchId: branchId!, name: form.name, price: parseFloat(form.price) || 0, description: form.description || undefined }),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ["crm-groups"] });
      setDialogOpen(false);
      toast({ title: t.groupAdded });
    },
    onError,
  });

  const updateMutation = useMutation({
    mutationFn: () =>
      groupsApi.update(editing!.id, { name: form.name, price: parseFloat(form.price) || 0, description: form.description || undefined }),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ["crm-groups"] });
      setDialogOpen(false);
      toast({ title: t.groupUpdated });
    },
    onError,
  });

  const toggleMutation = useMutation({
    mutationFn: ({ id, isActive }: { id: string; isActive: boolean }) => groupsApi.toggleActive(id, isActive),
    onSuccess: () => qc.invalidateQueries({ queryKey: ["crm-groups"] }),
    onError,
  });

  const openCreate = () => {
    setEditing(null);
    setForm(emptyForm);
    setDialogOpen(true);
  };

  const openEdit = (group: GroupDto) => {
    setEditing(group);
    setForm({ name: group.name, price: String(group.price), description: group.description ?? "" });
    setDialogOpen(true);
  };

  if (isLoading) return <PageLoader />;

  const columns: CrudColumn<GroupDto>[] = [
    {
      header: t.name,
      render: (group) => (
        <div>
          <div className="font-medium">{group.name}</div>
          {group.description && (
            <div className="text-xs text-muted-foreground line-clamp-1">{group.description}</div>
          )}
        </div>
      ),
    },
    {
      header: t.teacher,
      render: (group) => (
        <span className="text-sm">{group.teacherName || <span className="text-muted-foreground">—</span>}</span>
      ),
    },
    {
      header: t.studentsTitle,
      render: (group) => <span className="text-sm">{group.studentCount}</span>,
    },
    {
      header: t.price,
      render: (group) => <span className="text-sm">{group.price.toLocaleString()} so'm</span>,
    },
    {
      header: t.status,
      render: (group) => (
        <div className="flex items-center gap-2" onClick={(e) => e.stopPropagation()}>
          <Switch
            checked={group.isActive}
            onCheckedChange={(checked) => toggleMutation.mutate({ id: group.id, isActive: checked })}
          />
          <Badge variant={group.isActive ? "success" : "secondary"} className="text-xs">
            {group.isActive ? t.active : t.inactive}
          </Badge>
        </div>
      ),
    },
    {
      header: t.actions,
      className: "text-right w-24",
      render: (group) => (
        <div className="text-right" onClick={(e) => e.stopPropagation()}>
          <Button variant="ghost" size="icon" title={t.attendanceAction} onClick={() => setAttendanceGroup(group)}>
            <CalendarCheck className="h-4 w-4" />
          </Button>
          <Button variant="ghost" size="icon" onClick={() => openEdit(group)}>
            <Pencil className="h-4 w-4" />
          </Button>
        </div>
      ),
    },
  ];

  return (
    <div className="max-w-5xl mx-auto space-y-6">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-2xl font-bold">{t.groups}</h1>
          <p className="text-muted-foreground mt-1">{t.groupsSubtitle}</p>
        </div>
        <Button onClick={openCreate} disabled={!branchId}>
          <Plus className="h-4 w-4 mr-2" />
          {t.addGroup}
        </Button>
      </div>

      <CrudTable
        columns={columns}
        items={data?.items ?? []}
        getKey={(g) => g.id}
        emptyMessage={t.noGroups}
        onRowClick={(g) => navigate(`/crm/groups/${g.id}`)}
      />

      <CrudFormDialog
        open={dialogOpen}
        onOpenChange={setDialogOpen}
        title={editing ? t.editGroup : t.newGroup}
        onSave={() => (editing ? updateMutation.mutate() : createMutation.mutate())}
        saveDisabled={!form.name.trim() || createMutation.isPending || updateMutation.isPending}
        saving={createMutation.isPending || updateMutation.isPending}
      >
        <div className="space-y-1.5">
          <Label>{t.name}</Label>
          <Input value={form.name} onChange={(e) => setForm({ ...form, name: e.target.value })} />
        </div>
        <div className="space-y-1.5">
          <Label>{t.price} (so'm)</Label>
          <Input type="number" min="0" value={form.price} onChange={(e) => setForm({ ...form, price: e.target.value })} />
        </div>
        <div className="space-y-1.5">
          <Label>{t.description}</Label>
          <Textarea value={form.description} onChange={(e) => setForm({ ...form, description: e.target.value })} />
        </div>
      </CrudFormDialog>

      {attendanceGroup && (
        <AttendanceDialog
          groupId={attendanceGroup.id}
          groupName={attendanceGroup.name}
          onClose={() => setAttendanceGroup(null)}
        />
      )}
    </div>
  );
}
