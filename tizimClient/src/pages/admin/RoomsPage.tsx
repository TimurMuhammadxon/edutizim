import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import { useState } from "react";
import { roomsApi } from "@/api/rooms";
import { useBranchStore } from "@/store/branch";
import { Card, CardContent } from "@/components/ui/card";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Badge } from "@/components/ui/badge";
import { Switch } from "@/components/ui/switch";
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from "@/components/ui/table";
import { Dialog, DialogContent, DialogHeader, DialogTitle, DialogFooter } from "@/components/ui/dialog";
import { PageLoader } from "@/components/shared/LoadingSpinner";
import { toast } from "@/components/ui/use-toast";
import { Plus, Pencil } from "lucide-react";
import type { RoomDto } from "@/types";

type FormState = { name: string; capacity: string };
const emptyForm: FormState = { name: "", capacity: "" };

export function RoomsPage() {
  const qc = useQueryClient();
  const branchId = useBranchStore((s) => s.branchId);
  const [dialogOpen, setDialogOpen] = useState(false);
  const [editing, setEditing] = useState<RoomDto | null>(null);
  const [form, setForm] = useState<FormState>(emptyForm);

  const { data, isLoading } = useQuery({
    queryKey: ["org-rooms", branchId],
    queryFn: () => roomsApi.list({ branchId: branchId ?? undefined }),
  });

  const createMutation = useMutation({
    mutationFn: () => roomsApi.create({ branchId: branchId!, name: form.name, capacity: Number(form.capacity) || 1 }),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ["org-rooms"] });
      setDialogOpen(false);
      toast({ title: "Xona qo'shildi" });
    },
  });

  const updateMutation = useMutation({
    mutationFn: () => roomsApi.update(editing!.id, { name: form.name, capacity: Number(form.capacity) || 1 }),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ["org-rooms"] });
      setDialogOpen(false);
      toast({ title: "Xona yangilandi" });
    },
  });

  const toggleMutation = useMutation({
    mutationFn: ({ id, isActive }: { id: string; isActive: boolean }) => roomsApi.toggleActive(id, isActive),
    onSuccess: () => qc.invalidateQueries({ queryKey: ["org-rooms"] }),
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

  return (
    <div className="max-w-3xl mx-auto space-y-6">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-2xl font-bold">Xonalar</h1>
          <p className="text-muted-foreground mt-1">Filialdagi xonalar va sig'imi</p>
        </div>
        <Button onClick={openCreate} disabled={!branchId}>
          <Plus className="h-4 w-4 mr-2" />
          Xona qo'shish
        </Button>
      </div>

      <Card>
        <CardContent className="p-0">
          <Table>
            <TableHeader>
              <TableRow>
                <TableHead>Nomi</TableHead>
                <TableHead>Sig'imi</TableHead>
                <TableHead>Holat</TableHead>
                <TableHead className="text-right w-24">Amallar</TableHead>
              </TableRow>
            </TableHeader>
            <TableBody>
              {data?.map((room) => (
                <TableRow key={room.id}>
                  <TableCell className="font-medium">{room.name}</TableCell>
                  <TableCell className="text-sm">{room.capacity} kishi</TableCell>
                  <TableCell>
                    <div className="flex items-center gap-2">
                      <Switch
                        checked={room.isActive}
                        onCheckedChange={(checked) => toggleMutation.mutate({ id: room.id, isActive: checked })}
                      />
                      <Badge variant={room.isActive ? "success" : "secondary"} className="text-xs">
                        {room.isActive ? "Faol" : "Nofaol"}
                      </Badge>
                    </div>
                  </TableCell>
                  <TableCell className="text-right">
                    <Button variant="ghost" size="icon" onClick={() => openEdit(room)}>
                      <Pencil className="h-4 w-4" />
                    </Button>
                  </TableCell>
                </TableRow>
              ))}
              {data?.length === 0 && (
                <TableRow>
                  <TableCell colSpan={4} className="text-center text-muted-foreground py-10">
                    Xonalar yo'q
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
            <DialogTitle>{editing ? "Xonani tahrirlash" : "Yangi xona"}</DialogTitle>
          </DialogHeader>
          <div className="space-y-4 py-2">
            <div className="space-y-1.5">
              <Label>Nomi</Label>
              <Input value={form.name} onChange={(e) => setForm({ ...form, name: e.target.value })} />
            </div>
            <div className="space-y-1.5">
              <Label>Sig'imi (kishi)</Label>
              <Input type="number" min="1" value={form.capacity} onChange={(e) => setForm({ ...form, capacity: e.target.value })} />
            </div>
          </div>
          <DialogFooter>
            <Button variant="outline" onClick={() => setDialogOpen(false)}>Bekor</Button>
            <Button
              onClick={() => (editing ? updateMutation.mutate() : createMutation.mutate())}
              disabled={!form.name.trim() || !form.capacity || createMutation.isPending || updateMutation.isPending}
            >
              Saqlash
            </Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>
    </div>
  );
}
