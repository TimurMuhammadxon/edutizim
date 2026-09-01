import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import { useState } from "react";
import { useNavigate } from "react-router-dom";
import { groupsApi } from "@/api/groups";
import { useBranchStore } from "@/store/branch";
import { Card, CardContent } from "@/components/ui/card";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Textarea } from "@/components/ui/textarea";
import { Badge } from "@/components/ui/badge";
import { Switch } from "@/components/ui/switch";
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from "@/components/ui/table";
import { Dialog, DialogContent, DialogHeader, DialogTitle, DialogFooter } from "@/components/ui/dialog";
import { PageLoader } from "@/components/shared/LoadingSpinner";
import { AttendanceDialog } from "@/components/shared/AttendanceDialog";
import { toast } from "@/components/ui/use-toast";
import { Plus, Pencil, CalendarCheck } from "lucide-react";
import type { GroupDto } from "@/types";

type FormState = { name: string; price: string; description: string };
const emptyForm: FormState = { name: "", price: "", description: "" };

export function GroupsPage() {
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

  const createMutation = useMutation({
    mutationFn: () =>
      groupsApi.create({ branchId: branchId!, name: form.name, price: parseFloat(form.price) || 0, description: form.description || undefined }),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ["crm-groups"] });
      setDialogOpen(false);
      toast({ title: "Guruh qo'shildi" });
    },
  });

  const updateMutation = useMutation({
    mutationFn: () =>
      groupsApi.update(editing!.id, { name: form.name, price: parseFloat(form.price) || 0, description: form.description || undefined }),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ["crm-groups"] });
      setDialogOpen(false);
      toast({ title: "Guruh yangilandi" });
    },
  });

  const toggleMutation = useMutation({
    mutationFn: ({ id, isActive }: { id: string; isActive: boolean }) => groupsApi.toggleActive(id, isActive),
    onSuccess: () => qc.invalidateQueries({ queryKey: ["crm-groups"] }),
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

  return (
    <div className="max-w-5xl mx-auto space-y-6">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-2xl font-bold">Guruhlar</h1>
          <p className="text-muted-foreground mt-1">Guruhlarni, o'qituvchi va talabalarni boshqaring</p>
        </div>
        <Button onClick={openCreate} disabled={!branchId}>
          <Plus className="h-4 w-4 mr-2" />
          Guruh qo'shish
        </Button>
      </div>

      <Card>
        <CardContent className="p-0">
          <Table>
            <TableHeader>
              <TableRow>
                <TableHead>Nomi</TableHead>
                <TableHead>O'qituvchi</TableHead>
                <TableHead>Talabalar</TableHead>
                <TableHead>Narx</TableHead>
                <TableHead>Holat</TableHead>
                <TableHead className="text-right w-24">Amallar</TableHead>
              </TableRow>
            </TableHeader>
            <TableBody>
              {data?.items.map((group) => (
                <TableRow key={group.id} className="cursor-pointer" onClick={() => navigate(`/crm/groups/${group.id}`)}>
                  <TableCell>
                    <div className="font-medium">{group.name}</div>
                    {group.description && (
                      <div className="text-xs text-muted-foreground line-clamp-1">{group.description}</div>
                    )}
                  </TableCell>
                  <TableCell className="text-sm">{group.teacherName || <span className="text-muted-foreground">—</span>}</TableCell>
                  <TableCell className="text-sm">{group.studentCount} ta</TableCell>
                  <TableCell className="text-sm">{group.price.toLocaleString()} so'm</TableCell>
                  <TableCell onClick={(e) => e.stopPropagation()}>
                    <div className="flex items-center gap-2">
                      <Switch
                        checked={group.isActive}
                        onCheckedChange={(checked) => toggleMutation.mutate({ id: group.id, isActive: checked })}
                      />
                      <Badge variant={group.isActive ? "success" : "secondary"} className="text-xs">
                        {group.isActive ? "Faol" : "Nofaol"}
                      </Badge>
                    </div>
                  </TableCell>
                  <TableCell className="text-right" onClick={(e) => e.stopPropagation()}>
                    <Button variant="ghost" size="icon" title="Davomat" onClick={() => setAttendanceGroup(group)}>
                      <CalendarCheck className="h-4 w-4" />
                    </Button>
                    <Button variant="ghost" size="icon" onClick={() => openEdit(group)}>
                      <Pencil className="h-4 w-4" />
                    </Button>
                  </TableCell>
                </TableRow>
              ))}
              {data?.items.length === 0 && (
                <TableRow>
                  <TableCell colSpan={6} className="text-center text-muted-foreground py-10">
                    Guruhlar yo'q
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
            <DialogTitle>{editing ? "Guruhni tahrirlash" : "Yangi guruh"}</DialogTitle>
          </DialogHeader>
          <div className="space-y-4 py-2">
            <div className="space-y-1.5">
              <Label>Nomi</Label>
              <Input value={form.name} onChange={(e) => setForm({ ...form, name: e.target.value })} />
            </div>
            <div className="space-y-1.5">
              <Label>Narx (so'm)</Label>
              <Input type="number" min="0" value={form.price} onChange={(e) => setForm({ ...form, price: e.target.value })} />
            </div>
            <div className="space-y-1.5">
              <Label>Tavsif</Label>
              <Textarea value={form.description} onChange={(e) => setForm({ ...form, description: e.target.value })} />
            </div>
          </div>
          <DialogFooter>
            <Button variant="outline" onClick={() => setDialogOpen(false)}>Bekor</Button>
            <Button
              onClick={() => (editing ? updateMutation.mutate() : createMutation.mutate())}
              disabled={!form.name.trim() || createMutation.isPending || updateMutation.isPending}
            >
              Saqlash
            </Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>

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
