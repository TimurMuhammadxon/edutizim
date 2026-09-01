import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import { useState } from "react";
import { leadsApi } from "@/api/leads";
import { useBranchStore } from "@/store/branch";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Textarea } from "@/components/ui/textarea";
import { Badge } from "@/components/ui/badge";
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "@/components/ui/select";
import { PageLoader } from "@/components/shared/LoadingSpinner";
import { CrudPageHeader, CrudSearchBar } from "@/components/shared/CrudPageHeader";
import { CrudTable, type CrudColumn } from "@/components/shared/CrudTable";
import { CrudFormDialog } from "@/components/shared/CrudFormDialog";
import { toast } from "@/components/ui/use-toast";
import { getApiErrorMessage } from "@/lib/errors";
import { useTranslation } from "@/lib/i18n";
import { Pencil, UserCheck } from "lucide-react";
import type { LeadDto, ClientSource, LeadStage } from "@/types";

type FormState = { fullName: string; phone: string; email: string; notes: string; source: ClientSource };
const emptyForm: FormState = { fullName: "", phone: "", email: "", notes: "", source: "Other" };

export function LeadsPage() {
  const t = useTranslation();
  const qc = useQueryClient();
  const branchId = useBranchStore((s) => s.branchId);
  const [searchInput, setSearchInput] = useState("");
  const [search, setSearch] = useState("");
  const [dialogOpen, setDialogOpen] = useState(false);
  const [editing, setEditing] = useState<LeadDto | null>(null);
  const [form, setForm] = useState<FormState>(emptyForm);

  const SOURCES: { value: ClientSource; label: string }[] = [
    { value: "Instagram", label: t.sourceInstagram },
    { value: "Telegram", label: t.sourceTelegram },
    { value: "Website", label: t.sourceWebsite },
    { value: "Referral", label: t.sourceReferral },
    { value: "WalkIn", label: t.sourceWalkIn },
    { value: "Call", label: t.sourceCall },
    { value: "Other", label: t.sourceOther },
  ];

  const STAGES: { value: LeadStage; label: string; variant: "default" | "secondary" | "success" | "destructive" }[] = [
    { value: "New", label: t.stageNew, variant: "default" },
    { value: "Contacted", label: t.stageContacted, variant: "secondary" },
    { value: "TrialScheduled", label: t.stageTrialScheduled, variant: "secondary" },
    { value: "Negotiation", label: t.stageNegotiation, variant: "secondary" },
    { value: "Converted", label: t.stageConverted, variant: "success" },
    { value: "Lost", label: t.stageLost, variant: "destructive" },
  ];

  const sourceLabel = (s: string) => SOURCES.find((x) => x.value === s)?.label ?? s;
  const stageInfo = (s: string) => STAGES.find((x) => x.value === s) ?? STAGES[0];

  const { data, isLoading } = useQuery({
    queryKey: ["crm-leads", search, branchId],
    queryFn: () => leadsApi.list({ search: search || undefined, branchId: branchId ?? undefined, pageSize: 100 }),
  });

  const onError = (e: unknown) =>
    toast({ title: t.error, description: getApiErrorMessage(e), variant: "destructive" });

  const createMutation = useMutation({
    mutationFn: () =>
      leadsApi.create({ branchId: branchId!, fullName: form.fullName, phone: form.phone, source: form.source }),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ["crm-leads"] });
      setDialogOpen(false);
      toast({ title: t.leadAdded });
    },
    onError,
  });

  const updateMutation = useMutation({
    mutationFn: () =>
      leadsApi.update(editing!.id, {
        fullName: form.fullName,
        phone: form.phone,
        email: form.email || undefined,
        notes: form.notes || undefined,
      }),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ["crm-leads"] });
      setDialogOpen(false);
      toast({ title: t.leadUpdated });
    },
    onError,
  });

  const stageMutation = useMutation({
    mutationFn: ({ id, stage }: { id: string; stage: LeadStage }) => leadsApi.changeStage(id, stage),
    onSuccess: () => qc.invalidateQueries({ queryKey: ["crm-leads"] }),
    onError,
  });

  const convertMutation = useMutation({
    mutationFn: (id: string) => leadsApi.convertToStudent(id),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ["crm-leads"] });
      qc.invalidateQueries({ queryKey: ["crm-students"] });
      toast({ title: t.convertedToStudent });
    },
    onError,
  });

  const openCreate = () => {
    setEditing(null);
    setForm(emptyForm);
    setDialogOpen(true);
  };

  const openEdit = (lead: LeadDto) => {
    setEditing(lead);
    setForm({ fullName: lead.fullName, phone: lead.phone, email: lead.email ?? "", notes: lead.notes ?? "", source: lead.source });
    setDialogOpen(true);
  };

  if (isLoading) return <PageLoader />;

  const columns: CrudColumn<LeadDto>[] = [
    { header: t.fullName, render: (lead) => <span className="font-medium">{lead.fullName}</span> },
    { header: t.phoneNumber, render: (lead) => <span className="font-mono text-sm">{lead.phone}</span> },
    {
      header: t.source,
      render: (lead) => <Badge variant="secondary" className="text-xs">{sourceLabel(lead.source)}</Badge>,
    },
    {
      header: t.stage,
      render: (lead) => {
        const info = stageInfo(lead.stage);
        const converted = lead.stage === "Converted";
        return converted ? (
          <Badge variant={info.variant} className="text-xs">{info.label}</Badge>
        ) : (
          <Select value={lead.stage} onValueChange={(v) => stageMutation.mutate({ id: lead.id, stage: v as LeadStage })}>
            <SelectTrigger className="h-7 text-xs w-40"><SelectValue /></SelectTrigger>
            <SelectContent>
              {STAGES.filter((s) => s.value !== "Converted").map((s) => (
                <SelectItem key={s.value} value={s.value}>{s.label}</SelectItem>
              ))}
            </SelectContent>
          </Select>
        );
      },
    },
    {
      header: t.actions,
      className: "text-right w-32",
      render: (lead) => (
        <div className="text-right">
          {lead.stage !== "Converted" && (
            <Button
              variant="ghost"
              size="icon"
              title={t.convertToStudentAction}
              onClick={() => convertMutation.mutate(lead.id)}
              disabled={convertMutation.isPending}
            >
              <UserCheck className="h-4 w-4 text-emerald-500" />
            </Button>
          )}
          <Button variant="ghost" size="icon" onClick={() => openEdit(lead)}>
            <Pencil className="h-4 w-4" />
          </Button>
        </div>
      ),
    },
  ];

  return (
    <div className="max-w-6xl mx-auto space-y-6">
      <CrudPageHeader
        title={t.leadsTitle}
        count={data?.totalCount ?? 0}
        countLabel={t.total}
        addLabel={t.addLead}
        onAdd={openCreate}
        addDisabled={!branchId}
      />

      <CrudSearchBar
        value={searchInput}
        onChange={setSearchInput}
        onSearch={() => setSearch(searchInput)}
        placeholder={t.searchByNameOrPhone}
      />

      <CrudTable columns={columns} items={data?.items ?? []} getKey={(l) => l.id} emptyMessage={t.noLeadsFound} />

      <CrudFormDialog
        open={dialogOpen}
        onOpenChange={setDialogOpen}
        title={editing ? t.editLead : t.newLead}
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
        {editing && (
          <div className="space-y-1.5">
            <Label>{t.email}</Label>
            <Input type="email" value={form.email} onChange={(e) => setForm({ ...form, email: e.target.value })} />
          </div>
        )}
        {!editing && (
          <div className="space-y-1.5">
            <Label>{t.source}</Label>
            <Select value={form.source} onValueChange={(v) => setForm({ ...form, source: v as ClientSource })}>
              <SelectTrigger><SelectValue /></SelectTrigger>
              <SelectContent>
                {SOURCES.map((s) => (
                  <SelectItem key={s.value} value={s.value}>{s.label}</SelectItem>
                ))}
              </SelectContent>
            </Select>
          </div>
        )}
        {editing && (
          <div className="space-y-1.5">
            <Label>{t.crmNotes}</Label>
            <Textarea value={form.notes} onChange={(e) => setForm({ ...form, notes: e.target.value })} />
          </div>
        )}
      </CrudFormDialog>
    </div>
  );
}
