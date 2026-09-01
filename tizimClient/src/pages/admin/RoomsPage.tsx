import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import { useState } from "react";
import { roomsApi } from "@/api/rooms";
import { useBranchStore } from "@/store/branch";
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
import type { RoomDto } from "@/types";

type FormState = { name: string; capacity: string };
const emptyForm: FormState = { name: "", capacity: "" };

export function RoomsPage() {
  const t = useTranslation();
  const qc = useQueryClient();
  const branchId = useBranchStore((s) => s.branchId);
  const [dialogOpen, setDialogOpen] = useState(false);
  const [editing, setEditing] = useState<RoomDto | null>(null);
  const [form, setForm] = useState<FormState>(emptyForm);

  const { data, isLoading } = useQuery({
    queryKey: ["org-rooms", branchId],
    queryFn: () => roomsApi.list({ branchId: branchId ?? undefined }),
  });

  const onError = (e: unknown) =>
    toast({ title: t.error, description: getApiErrorMessage(e), variant: "destructive" });

  const createMutation = useMutation({
    mutationFn: () => roomsApi.create({ branchId: branchId!, name: form.name, capacity: Number(form.capacity) || 1 }),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ["org-rooms"] });
      setDialogOpen(false);
      toast({ title: t.roomAdded });
    },
    onError,
  });

  const updateMutation = useMutation({
    mutationFn: () => roomsApi.update(editing!.id, { name: form.name, capacity: Number(form.capacity) || 1 }),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ["org-rooms"] });
      setDialogOpen(false);
      toast({ title: t.roomUpdated });
    },
    onError,
  });

  const toggleMutation = useMutation({
    mutationFn: ({ id, isActive }: { id: string; isActive: boolean }) => roomsApi.toggleActive(id, isActive),
    onSuccess: () => qc.invalidateQueries({ queryKey: ["org-rooms"] }),
    onError,
  });

  const openCreate = () => {
    setEditing(null);
    setForm(emptyForm);
    setDialogOpen(true);
  };

  const openEdit = (room: RoomDto) => {
    setEditing(room);
    setForm({ name: room.name, capacity: String(room.capacity) });
    setDialogOpen(true);
  };

  if (isLoading) return <PageLoader />;

  const columns: CrudColumn<RoomDto>[] = [
    { header: t.name, render: (room) => <span className="font-medium">{room.name}</span> },
    { header: t.capacity, render: (room) => <span className="text-sm">{room.capacity} {t.capacityPeople}</span> },
    {
      header: t.status,
      render: (room) => (
        <div className="flex items-center gap-2">
          <Switch
            checked={room.isActive}
            onCheckedChange={(checked) => toggleMutation.mutate({ id: room.id, isActive: checked })}
          />
          <Badge variant={room.isActive ? "success" : "secondary"} className="text-xs">
            {room.isActive ? t.active : t.inactive}
          </Badge>
        </div>
      ),
    },
    {
      header: t.actions,
      className: "text-right w-24",
      render: (room) => (
        <div className="text-right">
          <Button variant="ghost" size="icon" onClick={() => openEdit(room)}>
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
          <h1 className="text-2xl font-bold">{t.roomsTitle}</h1>
          <p className="text-muted-foreground mt-1">{t.roomsSubtitle}</p>
        </div>
        <Button onClick={openCreate} disabled={!branchId}>
          <Plus className="h-4 w-4 mr-2" />
          {t.addRoom}
        </Button>
      </div>

      <CrudTable columns={columns} items={data ?? []} getKey={(r) => r.id} emptyMessage={t.noRooms} />

      <CrudFormDialog
        open={dialogOpen}
        onOpenChange={setDialogOpen}
        title={editing ? t.editRoom : t.newRoom}
        onSave={() => (editing ? updateMutation.mutate() : createMutation.mutate())}
        saveDisabled={!form.name.trim() || !form.capacity || createMutation.isPending || updateMutation.isPending}
        saving={createMutation.isPending || updateMutation.isPending}
      >
        <div className="space-y-1.5">
          <Label>{t.name}</Label>
          <Input value={form.name} onChange={(e) => setForm({ ...form, name: e.target.value })} />
        </div>
        <div className="space-y-1.5">
          <Label>{t.capacity} ({t.capacityPeople})</Label>
          <Input type="number" min="1" value={form.capacity} onChange={(e) => setForm({ ...form, capacity: e.target.value })} />
        </div>
      </CrudFormDialog>
    </div>
  );
}
