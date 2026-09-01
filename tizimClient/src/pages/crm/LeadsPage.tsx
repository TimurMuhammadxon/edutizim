import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import { useState } from "react";
import { leadsApi } from "@/api/leads";
import { useBranchStore } from "@/store/branch";
import { Card, CardContent } from "@/components/ui/card";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Textarea } from "@/components/ui/textarea";
import { Badge } from "@/components/ui/badge";
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from "@/components/ui/table";
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "@/components/ui/select";
import { Dialog, DialogContent, DialogHeader, DialogTitle, DialogFooter } from "@/components/ui/dialog";
import { PageLoader } from "@/components/shared/LoadingSpinner";
import { toast } from "@/components/ui/use-toast";
import { Plus, Pencil, Search, UserCheck } from "lucide-react";
import type { LeadDto, ClientSource, LeadStage } from "@/types";

const SOURCES: { value: ClientSource; label: string }[] = [
  { value: "Instagram", label: "Instagram" },
  { value: "Telegram", label: "Telegram" },
  { value: "Website", label: "Veb-sayt" },
  { value: "Referral", label: "Tavsiya" },
  { value: "WalkIn", label: "Bevosita tashrif" },
  { value: "Call", label: "Qo'ng'iroq" },
  { value: "Other", label: "Boshqa" },
];

const STAGES: { value: LeadStage; label: string; variant: "default" | "secondary" | "success" | "destructive" }[] = [
  { value: "New", label: "Yangi", variant: "default" },
  { value: "Contacted", label: "Bog'lanildi", variant: "secondary" },
  { value: "TrialScheduled", label: "Sinov darsi", variant: "secondary" },
  { value: "Negotiation", label: "Muzokara", variant: "secondary" },
  { value: "Converted", label: "Talabaga aylandi", variant: "success" },
  { value: "Lost", label: "Yo'qotildi", variant: "destructive" },
];

const sourceLabel = (s: string) => SOURCES.find((x) => x.value === s)?.label ?? s;
const stageInfo = (s: string) => STAGES.find((x) => x.value === s) ?? STAGES[0];

type FormState = { fullName: string; phone: string; email: string; notes: string; source: ClientSource };
const emptyForm: FormState = { fullName: "", phone: "", email: "", notes: "", source: "Other" };

export function LeadsPage() {
  const qc = useQueryClient();
  const branchId = useBranchStore((s) => s.branchId);
  const [searchInput, setSearchInput] = useState("");
  const [search, setSearch] = useState("");
  const [dialogOpen, setDialogOpen] = useState(false);
  const [editing, setEditing] = useState<LeadDto | null>(null);
  const [form, setForm] = useState<FormState>(emptyForm);

  const { data, isLoading } = useQuery({
    queryKey: ["crm-leads", search, branchId],
    queryFn: () => leadsApi.list({ search: search || undefined, branchId: branchId ?? undefined, pageSize: 100 }),
  });

  const createMutation = useMutation({
    mutationFn: () =>
      leadsApi.create({ branchId: branchId!, fullName: form.fullName, phone: form.phone, source: form.source }),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ["crm-leads"] });
      setDialogOpen(false);
      toast({ title: "Lid qo'shildi" });
    },
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
      toast({ title: "Lid yangilandi" });
    },
  });

  const stageMutation = useMutation({
    mutationFn: ({ id, stage }: { id: string; stage: LeadStage }) => leadsApi.changeStage(id, stage),
    onSuccess: () => qc.invalidateQueries({ queryKey: ["crm-leads"] }),
  });

  const convertMutation = useMutation({
    mutationFn: (id: string) => leadsApi.convertToStudent(id),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ["crm-leads"] });
      qc.invalidateQueries({ queryKey: ["crm-students"] });
      toast({ title: "Talabaga aylantirildi" });
    },
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

  return (
    <div className="max-w-6xl mx-auto space-y-6">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-2xl font-bold">Lidlar</h1>
          <p className="text-muted-foreground mt-1">Jami: {data?.totalCount ?? 0} ta</p>
        </div>
        <Button onClick={openCreate} disabled={!branchId}>
          <Plus className="h-4 w-4 mr-2" />
          Lid qo'shish
        </Button>
      </div>

      <div className="flex gap-2">
        <Input
          placeholder="Ism yoki telefon bo'yicha qidirish..."
          value={searchInput}
          onChange={(e) => setSearchInput(e.target.value)}
          onKeyDown={(e) => { if (e.key === "Enter") setSearch(searchInput); }}
          className="max-w-sm"
        />
        <Button variant="outline" onClick={() => setSearch(searchInput)}>
          <Search className="h-4 w-4" />
        </Button>
      </div>

      <Card>
        <CardContent className="p-0">
          <Table>
            <TableHeader>
              <TableRow>
                <TableHead>Ism</TableHead>
                <TableHead>Telefon</TableHead>
                <TableHead>Manba</TableHead>
                <TableHead>Bosqich</TableHead>
                <TableHead className="text-right w-32">Amallar</TableHead>
              </TableRow>
            </TableHeader>
            <TableBody>
              {data?.items.map((lead) => {
                const info = stageInfo(lead.stage);
                const converted = lead.stage === "Converted";
                return (
                  <TableRow key={lead.id}>
                    <TableCell className="font-medium">{lead.fullName}</TableCell>
                    <TableCell className="font-mono text-sm">{lead.phone}</TableCell>
                    <TableCell>
                      <Badge variant="secondary" className="text-xs">{sourceLabel(lead.source)}</Badge>
                    </TableCell>
                    <TableCell>
                      {converted ? (
                        <Badge variant={info.variant} className="text-xs">{info.label}</Badge>
                      ) : (
                        <Select
                          value={lead.stage}
                          onValueChange={(v) => stageMutation.mutate({ id: lead.id, stage: v as LeadStage })}
                        >
                          <SelectTrigger className="h-7 text-xs w-40"><SelectValue /></SelectTrigger>
                          <SelectContent>
                            {STAGES.filter((s) => s.value !== "Converted").map((s) => (
                              <SelectItem key={s.value} value={s.value}>{s.label}</SelectItem>
                            ))}
                          </SelectContent>
                        </Select>
                      )}
                    </TableCell>
                    <TableCell className="text-right">
                      {!converted && (
                        <Button
                          variant="ghost"
                          size="icon"
                          title="Talabaga aylantirish"
                          onClick={() => convertMutation.mutate(lead.id)}
                          disabled={convertMutation.isPending}
                        >
                          <UserCheck className="h-4 w-4 text-emerald-500" />
                        </Button>
                      )}
                      <Button variant="ghost" size="icon" onClick={() => openEdit(lead)}>
                        <Pencil className="h-4 w-4" />
                      </Button>
                    </TableCell>
                  </TableRow>
                );
              })}
              {data?.items.length === 0 && (
                <TableRow>
                  <TableCell colSpan={5} className="text-center text-muted-foreground py-10">
                    Lidlar topilmadi
                  </TableCell>
                </TableRow>
              )}
            </TableBody>
          </Table>
        </CardContent>
      </Card>

      <Dialog open={dialogOpen} onOpenChange={setDialogOpen}>
        <DialogContent>
          <DialogHeader>
            <DialogTitle>{editing ? "Lidni tahrirlash" : "Yangi lid"}</DialogTitle>
          </DialogHeader>
          <div className="space-y-4 py-2">
            <div className="space-y-1.5">
              <Label>Ism familiya</Label>
              <Input value={form.fullName} onChange={(e) => setForm({ ...form, fullName: e.target.value })} />
            </div>
            <div className="space-y-1.5">
              <Label>Telefon</Label>
              <Input value={form.phone} onChange={(e) => setForm({ ...form, phone: e.target.value })} placeholder="+998 XX XXX XX XX" />
            </div>
            {editing && (
              <div className="space-y-1.5">
                <Label>Email</Label>
                <Input type="email" value={form.email} onChange={(e) => setForm({ ...form, email: e.target.value })} />
              </div>
            )}
            {!editing && (
              <div className="space-y-1.5">
                <Label>Manba</Label>
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
                <Label>Izoh</Label>
                <Textarea value={form.notes} onChange={(e) => setForm({ ...form, notes: e.target.value })} />
              </div>
            )}
          </div>
          <DialogFooter>
            <Button variant="outline" onClick={() => setDialogOpen(false)}>Bekor</Button>
            <Button
              onClick={() => (editing ? updateMutation.mutate() : createMutation.mutate())}
              disabled={!form.fullName.trim() || !form.phone.trim() || createMutation.isPending || updateMutation.isPending}
            >
              Saqlash
            </Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>
    </div>
  );
}
