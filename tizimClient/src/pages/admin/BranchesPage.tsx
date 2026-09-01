import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import { useState } from "react";
import { branchesApi } from "@/api/branches";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Badge } from "@/components/ui/badge";
import { Switch } from "@/components/ui/switch";
import { PageLoader } from "@/components/shared/LoadingSpinner";
import { CrudTable, type CrudColumn } from "@/components/shared/CrudTable";
import { CrudFormDialog } from "@/components/shared/CrudFormDialog";
import { toast } from "@/components/ui/use-toast";
import { getApiErrorMessage } from "@/lib/errors";
import { useTranslation } from "@/lib/i18n";
import { Plus, Pencil } from "lucide-react";
import type { BranchDto } from "@/types";

type FormState = { name: string; address: string };
const emptyForm: FormState = { name: "", address: "" };

export function BranchesPage() {
  const t = useTranslation();
  const qc = useQueryClient();
  const [dialogOpen, setDialogOpen] = useState(false);
  const [editing, setEditing] = useState<BranchDto | null>(null);
  const [form, setForm] = useState<FormState>(emptyForm);

  const { data, isLoading } = useQuery({
    queryKey: ["org-branches"],
    queryFn: () => branchesApi.list(),
  });

  const onError = (e: unknown) =>
    toast({ title: t.error, description: getApiErrorMessage(e), variant: "destructive" });

  const createMutation = useMutation({
    mutationFn: () => branchesApi.create({ name: form.name, address: form.address || undefined }),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ["org-branches"] });
      setDialogOpen(false);
      toast({ title: t.branchAdded });
    },
    onError,
  });

  const updateMutation = useMutation({
    mutationFn: () => branchesApi.update(editing!.id, { name: form.name, address: form.address || undefined }),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ["org-branches"] });
      setDialogOpen(false);
      toast({ title: t.branchUpdated });
    },
    onError,
  });

  const toggleMutation = useMutation({
    mutationFn: ({ id, isActive }: { id: string; isActive: boolean }) => branchesApi.toggleActive(id, isActive),
    onSuccess: () => qc.invalidateQueries({ queryKey: ["org-branches"] }),
    onError,
  });

  const openCreate = () => {
    setEditing(null);
    setForm(emptyForm);
    setDialogOpen(true);
  };

  const openEdit = (branch: BranchDto) => {
    setEditing(branch);
    setForm({ name: branch.name, address: branch.address ?? "" });
    setDialogOpen(true);
  };

  if (isLoading) return <PageLoader />;

  const columns: CrudColumn<BranchDto>[] = [
    { header: t.name, render: (branch) => <span className="font-medium">{branch.name}</span> },
    {
      header: t.address,
      render: (branch) => <span className="text-sm text-muted-foreground">{branch.address ?? "—"}</span>,
    },
    {
      header: t.status,
      render: (branch) => (
        <div className="flex items-center gap-2">
          <Switch
            checked={branch.isActive}
            onCheckedChange={(checked) => toggleMutation.mutate({ id: branch.id, isActive: checked })}
          />
          <Badge variant={branch.isActive ? "success" : "secondary"} className="text-xs">
            {branch.isActive ? t.active : t.inactive}
          </Badge>
        </div>
      ),
    },
    {
      header: t.actions,
      className: "text-right w-24",
      render: (branch) => (
        <div className="text-right">
          <Button variant="ghost" size="icon" onClick={() => openEdit(branch)}>
            <Pencil className="h-4 w-4" />
          </Button>
        </div>
      ),
    },
  ];

  return (
    <div className="max-w-3xl mx-auto space-y-6">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-2xl font-bold">{t.branchesTitle}</h1>
          <p className="text-muted-foreground mt-1">{t.branchesSubtitle}</p>
        </div>
        <Button onClick={openCreate}>
          <Plus className="h-4 w-4 mr-2" />
          {t.addBranch}
        </Button>
      </div>

      <CrudTable columns={columns} items={data ?? []} getKey={(b) => b.id} emptyMessage={t.noBranches} />

      <CrudFormDialog
        open={dialogOpen}
        onOpenChange={setDialogOpen}
        title={editing ? t.editBranch : t.newBranch}
        onSave={() => (editing ? updateMutation.mutate() : createMutation.mutate())}
        saveDisabled={!form.name.trim() || createMutation.isPending || updateMutation.isPending}
        saving={createMutation.isPending || updateMutation.isPending}
      >
        <div className="space-y-1.5">
          <Label>{t.name}</Label>
          <Input value={form.name} onChange={(e) => setForm({ ...form, name: e.target.value })} />
        </div>
        <div className="space-y-1.5">
          <Label>{t.address}</Label>
          <Input value={form.address} onChange={(e) => setForm({ ...form, address: e.target.value })} />
        </div>
      </CrudFormDialog>
    </div>
  );
}
