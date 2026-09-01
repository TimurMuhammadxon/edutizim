import { useQuery, useMutation, useQueryClient, keepPreviousData } from "@tanstack/react-query";
import { useState, useEffect } from "react";
import { useNavigate } from "react-router-dom";
import { studentsApi } from "@/api/students";
import { financeApi } from "@/api/finance";
import { groupsApi } from "@/api/groups";
import { useBranchStore } from "@/store/branch";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Textarea } from "@/components/ui/textarea";
import { Badge } from "@/components/ui/badge";
import { Switch } from "@/components/ui/switch";
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "@/components/ui/select";
import { Dialog, DialogContent, DialogHeader, DialogTitle, DialogFooter } from "@/components/ui/dialog";
import { PageLoader } from "@/components/shared/LoadingSpinner";
import { CrudPageHeader } from "@/components/shared/CrudPageHeader";
import { CrudTable, type CrudColumn } from "@/components/shared/CrudTable";
import { CrudFormDialog } from "@/components/shared/CrudFormDialog";
import { toast } from "@/components/ui/use-toast";
import { getApiErrorMessage } from "@/lib/errors";
import { useTranslation } from "@/lib/i18n";
import { Search, Pencil, KeyRound } from "lucide-react";
import { formatPhone } from "@/lib/groupHelpers";
import type { StudentDto } from "@/types";

type FormState = { fullName: string; phone: string; email: string; notes: string };
const emptyForm: FormState = { fullName: "", phone: "", email: "", notes: "" };

export function StudentsPage() {
  const t = useTranslation();
  const qc = useQueryClient();
  const navigate = useNavigate();
  const branchId = useBranchStore((s) => s.branchId);
  const [searchInput, setSearchInput] = useState("");
  const [search, setSearch] = useState("");
  const [groupFilter, setGroupFilter] = useState("all");
  const [financialFilter, setFinancialFilter] = useState("all");
  const [studentStatusFilter, setStudentStatusFilter] = useState("all");
  const [dialogOpen, setDialogOpen] = useState(false);
  const [editing, setEditing] = useState<StudentDto | null>(null);
  const [form, setForm] = useState<FormState>(emptyForm);
  const [loginStudent, setLoginStudent] = useState<StudentDto | null>(null);
  const [loginPassword, setLoginPassword] = useState("");

  const FINANCIAL_STATUS_OPTIONS = [
    { value: "WithDebt", label: t.financialWithDebt },
    { value: "WithDiscount", label: t.financialWithDiscount },
    { value: "WithoutDebt", label: t.financialWithoutDebt },
    { value: "PositiveBalance", label: t.financialPositiveBalance },
    { value: "PaidThisMonth", label: t.financialPaidThisMonth },
  ];

  const STUDENT_STATUS_OPTIONS = [
    { value: "AddedThisMonth", label: t.studentAddedThisMonth },
    { value: "Trial", label: t.studentTrial },
    { value: "Active", label: t.studentActive },
    { value: "Frozen", label: t.studentFrozen },
    { value: "WithoutGroup", label: t.studentWithoutGroup },
    { value: "LeftAfterTrial", label: t.studentLeftAfterTrial },
  ];

  useEffect(() => {
    const timer = setTimeout(() => setSearch(searchInput), 250);
    return () => clearTimeout(timer);
  }, [searchInput]);

  const { data, isLoading } = useQuery({
    queryKey: ["crm-students", search, branchId, groupFilter, financialFilter, studentStatusFilter],
    queryFn: () => studentsApi.list({
      search: search || undefined,
      branchId: branchId ?? undefined,
      groupId: groupFilter === "all" ? undefined : groupFilter,
      financialStatus: financialFilter === "all" ? undefined : financialFilter,
      studentStatus: studentStatusFilter === "all" ? undefined : studentStatusFilter,
      pageSize: 100,
    }),
    placeholderData: keepPreviousData,
  });

  const { data: groupOptions } = useQuery({
    queryKey: ["crm-groups-filter", branchId],
    queryFn: () => groupsApi.list({ branchId: branchId ?? undefined, pageSize: 200 }),
  });

  const { data: debtorsResult } = useQuery({
    queryKey: ["finance-debtors"],
    queryFn: () => financeApi.getDebtors({ pageSize: 200 }),
  });
  const debtorStudentIds = new Set(debtorsResult?.items.map((d) => d.studentId));

  const onError = (e: unknown) =>
    toast({ title: t.error, description: getApiErrorMessage(e), variant: "destructive" });

  const createMutation = useMutation({
    mutationFn: () =>
      studentsApi.create({ branchId: branchId!, fullName: form.fullName, phone: form.phone, email: form.email || undefined }),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ["crm-students"] });
      setDialogOpen(false);
      toast({ title: t.studentAdded });
    },
    onError,
  });

  const updateMutation = useMutation({
    mutationFn: () =>
      studentsApi.update(editing!.id, {
        fullName: form.fullName,
        phone: form.phone,
        email: form.email || undefined,
        notes: form.notes || undefined,
      }),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ["crm-students"] });
      setDialogOpen(false);
      toast({ title: t.studentUpdated });
    },
    onError,
  });

  const toggleMutation = useMutation({
    mutationFn: ({ id, isActive }: { id: string; isActive: boolean }) => studentsApi.toggleActive(id, isActive),
    onSuccess: () => qc.invalidateQueries({ queryKey: ["crm-students"] }),
    onError,
  });

  const createLoginMutation = useMutation({
    mutationFn: () => studentsApi.createLogin(loginStudent!.id, loginPassword),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ["crm-students"] });
      setLoginStudent(null);
      setLoginPassword("");
      toast({ title: t.loginCreated });
    },
    onError,
  });

  const openCreate = () => {
    setEditing(null);
    setForm(emptyForm);
    setDialogOpen(true);
  };

  const openEdit = (student: StudentDto) => {
    setEditing(student);
    setForm({ fullName: student.fullName, phone: student.phone, email: student.email ?? "", notes: student.notes ?? "" });
    setDialogOpen(true);
  };

  if (isLoading) return <PageLoader />;

  const columns: CrudColumn<StudentDto>[] = [
    {
      header: t.fullName,
      render: (student) => (
        <span className="font-medium">
          {student.fullName}
          {student.leadId && <Badge variant="secondary" className="text-xs ml-2">{t.viaLead}</Badge>}
          {debtorStudentIds.has(student.id) && <Badge variant="destructive" className="text-xs ml-2">{t.debtorBadge}</Badge>}
        </span>
      ),
    },
    {
      header: t.phoneNumber,
      render: (student) => <span className="font-mono text-base font-medium tracking-wide">{formatPhone(student.phone)}</span>,
    },
    {
      header: t.status,
      render: (student) => (
        <div className="flex items-center gap-2" onClick={(e) => e.stopPropagation()}>
          <Switch
            checked={student.isActive}
            onCheckedChange={(checked) => toggleMutation.mutate({ id: student.id, isActive: checked })}
          />
          <Badge variant={student.isActive ? "success" : "secondary"} className="text-xs">
            {student.isActive ? t.studying : t.left}
          </Badge>
        </div>
      ),
    },
    {
      header: t.joined,
      render: (student) => (
        <span className="text-sm text-muted-foreground">{new Date(student.createdAt).toLocaleDateString("ru-RU")}</span>
      ),
    },
    {
      header: t.actions,
      className: "text-right w-24",
      render: (student) => (
        <div className="text-right" onClick={(e) => e.stopPropagation()}>
          {student.userId ? (
            <Badge variant="success" className="text-xs mr-1">{t.hasLogin}</Badge>
          ) : (
            <Button
              variant="ghost"
              size="icon"
              title={t.createLoginAction}
              onClick={() => { setLoginStudent(student); setLoginPassword(""); }}
            >
              <KeyRound className="h-4 w-4 text-cyan-500" />
            </Button>
          )}
          <Button variant="ghost" size="icon" onClick={() => openEdit(student)}>
            <Pencil className="h-4 w-4" />
          </Button>
        </div>
      ),
    },
  ];

  return (
    <div className="max-w-5xl mx-auto space-y-6">
      <CrudPageHeader
        title={t.studentsTitle}
        count={data?.totalCount ?? 0}
        countLabel={t.total}
        addLabel={t.addStudent}
        onAdd={openCreate}
        addDisabled={!branchId}
      />

      <div className="flex flex-wrap gap-2">
        <div className="relative max-w-sm flex-1 min-w-[200px]">
          <Search className="h-4 w-4 absolute left-2.5 top-1/2 -translate-y-1/2 text-muted-foreground" />
          <Input
            placeholder={t.searchByNameOrPhone}
            value={searchInput}
            onChange={(e) => setSearchInput(e.target.value)}
            className="pl-8"
          />
        </div>

        <Select value={groupFilter} onValueChange={setGroupFilter}>
          <SelectTrigger className="w-44">
            <SelectValue placeholder={t.groups} />
          </SelectTrigger>
          <SelectContent>
            <SelectItem value="all">{t.allGroups}</SelectItem>
            {groupOptions?.items.map((g) => (
              <SelectItem key={g.id} value={g.id}>{g.name}</SelectItem>
            ))}
          </SelectContent>
        </Select>

        <Select value={financialFilter} onValueChange={setFinancialFilter}>
          <SelectTrigger className="w-48">
            <SelectValue placeholder={t.financialStatusLabel} />
          </SelectTrigger>
          <SelectContent>
            <SelectItem value="all">{t.allFinancialStatuses}</SelectItem>
            {FINANCIAL_STATUS_OPTIONS.map((o) => (
              <SelectItem key={o.value} value={o.value}>{o.label}</SelectItem>
            ))}
          </SelectContent>
        </Select>

        <Select value={studentStatusFilter} onValueChange={setStudentStatusFilter}>
          <SelectTrigger className="w-48">
            <SelectValue placeholder={t.studentStatusLabel} />
          </SelectTrigger>
          <SelectContent>
            <SelectItem value="all">{t.allStudentStatuses}</SelectItem>
            {STUDENT_STATUS_OPTIONS.map((o) => (
              <SelectItem key={o.value} value={o.value}>{o.label}</SelectItem>
            ))}
          </SelectContent>
        </Select>
      </div>

      <CrudTable
        columns={columns}
        items={data?.items ?? []}
        getKey={(s) => s.id}
        emptyMessage={t.noStudentsFound}
        onRowClick={(s) => navigate(`/crm/students/${s.id}`)}
      />

      <CrudFormDialog
        open={dialogOpen}
        onOpenChange={setDialogOpen}
        title={editing ? t.editStudent : t.newStudent}
        onSave={() => (editing ? updateMutation.mutate() : createMutation.mutate())}
        saveDisabled={!form.fullName.trim() || !form.phone.trim() || createMutation.isPending || updateMutation.isPending}
        saving={createMutation.isPending || updateMutation.isPending}
      >
        <div className="space-y-1.5">
          <Label>{t.fullName}</Label>
          <Input value={form.fullName} onChange={(e) => setForm({ ...form, fullName: e.target.value })} />
        </div>
        <div className="space-y-1.5">
          <Label>{t.phoneNumber}</Label>
          <Input value={form.phone} onChange={(e) => setForm({ ...form, phone: e.target.value })} placeholder="+998 XX XXX XX XX" />
        </div>
        <div className="space-y-1.5">
          <Label>{t.email}</Label>
          <Input type="email" value={form.email} onChange={(e) => setForm({ ...form, email: e.target.value })} />
        </div>
        {editing && (
          <div className="space-y-1.5">
            <Label>{t.crmNotes}</Label>
            <Textarea value={form.notes} onChange={(e) => setForm({ ...form, notes: e.target.value })} />
          </div>
        )}
      </CrudFormDialog>

      <Dialog open={!!loginStudent} onOpenChange={(o) => !o && setLoginStudent(null)}>
        <DialogContent>
          <DialogHeader>
            <DialogTitle>{t.createLoginTitle}</DialogTitle>
          </DialogHeader>
          <div className="space-y-4 py-2">
            <div className="space-y-1.5">
              <Label>{t.loginPhoneLabel}</Label>
              <Input value={loginStudent?.phone ?? ""} disabled />
            </div>
            <div className="space-y-1.5">
              <Label>{t.passwordLabel}</Label>
              <Input
                type="text"
                value={loginPassword}
                onChange={(e) => setLoginPassword(e.target.value)}
                placeholder={t.minSixChars}
              />
            </div>
          </div>
          <DialogFooter>
            <Button variant="outline" onClick={() => setLoginStudent(null)}>{t.cancel}</Button>
            <Button
              onClick={() => createLoginMutation.mutate()}
              disabled={loginPassword.length < 6 || createLoginMutation.isPending}
            >
              {t.createAction}
            </Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>
    </div>
  );
}
