import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import { useState } from "react";
import { membersApi } from "@/api/members";
import { useAuthStore } from "@/store/auth";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Badge } from "@/components/ui/badge";
import { PageLoader } from "@/components/shared/LoadingSpinner";
import { CrudTable, type CrudColumn } from "@/components/shared/CrudTable";
import { CrudFormDialog } from "@/components/shared/CrudFormDialog";
import { toast } from "@/components/ui/use-toast";
import { getApiErrorMessage } from "@/lib/errors";
import { useTranslation } from "@/lib/i18n";
import { Plus, UserX } from "lucide-react";
import type { MemberDto, Role } from "@/types";

type FormState = { firstName: string; lastName: string; phone: string; password: string };
const emptyForm: FormState = { firstName: "", lastName: "", phone: "", password: "" };

export function StaffPage() {
  const t = useTranslation();
  const qc = useQueryClient();
  const user = useAuthStore((s) => s.user);
  const isOrgAdmin = user && ["OrgAdmin", "SuperAdmin", "Owner"].includes(user.role);
  const [dialogRole, setDialogRole] = useState<Role | null>(null);
  const [form, setForm] = useState<FormState>(emptyForm);

  const ROLE_LABELS: Record<string, string> = { Staff: t.roleStaffLabel, Teacher: t.roleTeacherLabel };

  const { data, isLoading } = useQuery({
    queryKey: ["org-members"],
    queryFn: () => membersApi.list(),
  });

  const onError = (e: unknown) =>
    toast({ title: t.error, description: getApiErrorMessage(e), variant: "destructive" });

  const createStaffMutation = useMutation({
    mutationFn: () =>
      membersApi.createStaff({
        phone: form.phone, password: form.password,
        firstName: form.firstName || undefined, lastName: form.lastName || undefined,
      }),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ["org-members"] });
      setDialogRole(null);
      toast({ title: t.staffAdded });
    },
    onError,
  });

  const createTeacherMutation = useMutation({
    mutationFn: () =>
      membersApi.createTeacher({
        phone: form.phone, password: form.password,
        firstName: form.firstName || undefined, lastName: form.lastName || undefined,
      }),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ["org-members"] });
      setDialogRole(null);
      toast({ title: t.teacherAdded });
    },
    onError,
  });

  const deactivateMutation = useMutation({
    mutationFn: (id: string) => membersApi.deactivate(id),
    onSuccess: () => qc.invalidateQueries({ queryKey: ["org-members"] }),
    onError,
  });

  const openCreate = (role: Role) => {
    setForm(emptyForm);
    setDialogRole(role);
  };

  const submitting = createStaffMutation.isPending || createTeacherMutation.isPending;

  const handleSubmit = () => {
    if (dialogRole === "Staff") createStaffMutation.mutate();
    else if (dialogRole === "Teacher") createTeacherMutation.mutate();
  };

  if (isLoading) return <PageLoader />;

  const columns: CrudColumn<MemberDto>[] = [
    { header: t.fullName, render: (m) => <span className="font-medium">{m.fullName?.trim() || "—"}</span> },
    { header: t.phoneNumber, render: (m) => <span className="font-mono text-sm">{m.phone ?? "—"}</span> },
    {
      header: t.roleColumn,
      render: (m) => <Badge variant="secondary" className="text-xs">{ROLE_LABELS[m.role] ?? m.role}</Badge>,
    },
    {
      header: t.status,
      render: (m) => (
        <Badge variant={m.isActive ? "success" : "secondary"} className="text-xs">
          {m.isActive ? t.active : t.inactive}
        </Badge>
      ),
    },
    ...(isOrgAdmin
      ? [{
          header: t.actions,
          className: "text-right w-20",
          render: (m: MemberDto) => (
            <div className="text-right">
              {m.isActive && (
                <Button variant="ghost" size="icon" onClick={() => deactivateMutation.mutate(m.id)} title={t.deactivateAction}>
                  <UserX className="h-4 w-4 text-red-500" />
                </Button>
              )}
            </div>
          ),
        }]
      : []),
  ];

  return (
    <div className="max-w-4xl mx-auto space-y-6">
      <div className="flex items-center justify-between flex-wrap gap-3">
        <div>
          <h1 className="text-2xl font-bold">{t.staffTitle}</h1>
          <p className="text-muted-foreground mt-1">{t.total}: {data?.length ?? 0}</p>
        </div>
        <div className="flex gap-2">
          {isOrgAdmin && (
            <Button variant="outline" onClick={() => openCreate("Staff")}>
              <Plus className="h-4 w-4 mr-2" />
              {t.addStaff}
            </Button>
          )}
          <Button onClick={() => openCreate("Teacher")}>
            <Plus className="h-4 w-4 mr-2" />
            {t.addTeacher}
          </Button>
        </div>
      </div>

      <CrudTable columns={columns} items={data ?? []} getKey={(m) => m.id} emptyMessage={t.noStaffFound} />

      <CrudFormDialog
        open={dialogRole !== null}
        onOpenChange={(o) => !o && setDialogRole(null)}
        title={dialogRole === "Staff" ? t.newStaffMember : t.newTeacherMember}
        onSave={handleSubmit}
        saveDisabled={!form.phone.trim() || form.password.length < 6 || submitting}
        saving={submitting}
      >
        <div className="space-y-1.5">
          <Label>{t.firstName}</Label>
          <Input value={form.firstName} onChange={(e) => setForm({ ...form, firstName: e.target.value })} />
        </div>
        <div className="space-y-1.5">
          <Label>{t.lastName}</Label>
          <Input value={form.lastName} onChange={(e) => setForm({ ...form, lastName: e.target.value })} />
        </div>
        <div className="space-y-1.5">
          <Label>{t.phoneLoginLabel}</Label>
          <Input value={form.phone} onChange={(e) => setForm({ ...form, phone: e.target.value })} placeholder="+998 XX XXX XX XX" />
        </div>
        <div className="space-y-1.5">
          <Label>{t.passwordLabel}</Label>
          <Input type="text" value={form.password} onChange={(e) => setForm({ ...form, password: e.target.value })} placeholder={t.minSixChars} />
        </div>
      </CrudFormDialog>
    </div>
  );
}
