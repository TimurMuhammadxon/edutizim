import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import { useState } from "react";
import { branchesApi } from "@/api/branches";
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
import type { BranchDto } from "@/types";

type FormState = { name: string; address: string };
const emptyForm: FormState = { name: "", address: "" };

export function BranchesPage() {
  const qc = useQueryClient();
  const [dialogOpen, setDialogOpen] = useState(false);
  const [editing, setEditing] = useState<BranchDto | null>(null);
  const [form, setForm] = useState<FormState>(emptyForm);

  const { data, isLoading } = useQuery({
    queryKey: ["org-branches"],
    queryFn: () => branchesApi.list(),
  });

  const createMutation = useMutation({
    mutationFn: () => branchesApi.create({ name: form.name, address: form.address || undefined }),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ["org-branches"] });
      setDialogOpen(false);
      toast({ title: "Filial qo'shildi" });
    },
  });

  const updateMutation = useMutation({
    mutationFn: () => branchesApi.update(editing!.id, { name: form.name, address: form.address || undefined }),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ["org-branches"] });
      setDialogOpen(false);
      toast({ title: "Filial yangilandi" });
    },
  });

  const toggleMutation = useMutation({
    mutationFn: ({ id, isActive }: { id: string; isActive: boolean }) => branchesApi.toggleActive(id, isActive),
    onSuccess: () => qc.invalidateQueries({ queryKey: ["org-branches"] }),
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

  return (
    <div className="max-w-3xl mx-auto space-y-6">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-2xl font-bold">Filiallar</h1>
          <p className="text-muted-foreground mt-1">Fiziki filiallaringizni boshqaring</p>
        </div>
        <Button onClick={openCreate}>
          <Plus className="h-4 w-4 mr-2" />
          Filial qo'shish
        </Button>
      </div>

      <Card>
        <CardContent className="p-0">
          <Table>
            <TableHeader>
              <TableRow>
                <TableHead>Nomi</TableHead>
                <TableHead>Manzil</TableHead>
                <TableHead>Holat</TableHead>
                <TableHead className="text-right w-24">Amallar</TableHead>
              </TableRow>
            </TableHeader>
            <TableBody>
              {data?.map((branch) => (
                <TableRow key={branch.id}>
                  <TableCell className="font-medium">{branch.name}</TableCell>
                  <TableCell className="text-sm text-muted-foreground">{branch.address ?? "—"}</TableCell>
                  <TableCell>
                    <div className="flex items-center gap-2">
                      <Switch
                        checked={branch.isActive}
                        onCheckedChange={(checked) => toggleMutation.mutate({ id: branch.id, isActive: checked })}
                      />
                      <Badge variant={branch.isActive ? "success" : "secondary"} className="text-xs">
                        {branch.isActive ? "Faol" : "Nofaol"}
                      </Badge>
                    </div>
                  </TableCell>
                  <TableCell className="text-right">
                    <Button variant="ghost" size="icon" onClick={() => openEdit(branch)}>
                      <Pencil className="h-4 w-4" />
                    </Button>
                  </TableCell>
                </TableRow>
              ))}
              {data?.length === 0 && (
                <TableRow>
                  <TableCell colSpan={4} className="text-center text-muted-foreground py-10">
                    Filiallar yo'q
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
            <DialogTitle>{editing ? "Filialni tahrirlash" : "Yangi filial"}</DialogTitle>
          </DialogHeader>
          <div className="space-y-4 py-2">
            <div className="space-y-1.5">
              <Label>Nomi</Label>
              <Input value={form.name} onChange={(e) => setForm({ ...form, name: e.target.value })} />
            </div>
            <div className="space-y-1.5">
              <Label>Manzil</Label>
              <Input value={form.address} onChange={(e) => setForm({ ...form, address: e.target.value })} />
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
    </div>
  );
}
