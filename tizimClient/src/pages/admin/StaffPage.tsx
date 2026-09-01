import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import { useState } from "react";
import { membersApi } from "@/api/members";
import { useAuthStore } from "@/store/auth";
import { Card, CardContent } from "@/components/ui/card";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Badge } from "@/components/ui/badge";
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from "@/components/ui/table";
import { Dialog, DialogContent, DialogHeader, DialogTitle, DialogFooter } from "@/components/ui/dialog";
import { PageLoader } from "@/components/shared/LoadingSpinner";
import { toast } from "@/components/ui/use-toast";
import { Plus, UserX } from "lucide-react";
import type { MemberDto, Role } from "@/types";

type FormState = { firstName: string; lastName: string; phone: string; password: string };
const emptyForm: FormState = { firstName: "", lastName: "", phone: "", password: "" };

const ROLE_LABELS: Record<string, string> = { Staff: "Xodim", Teacher: "O'qituvchi" };

export function StaffPage() {
  const qc = useQueryClient();
  const user = useAuthStore((s) => s.user);
  const isOrgAdmin = user && ["OrgAdmin", "SuperAdmin", "Owner"].includes(user.role);
  const [dialogRole, setDialogRole] = useState<Role | null>(null);
  const [form, setForm] = useState<FormState>(emptyForm);

  const { data, isLoading } = useQuery({
    queryKey: ["org-members"],
    queryFn: () => membersApi.list(),
  });

  const createStaffMutation = useMutation({
    mutationFn: () =>
      membersApi.createStaff({
        phone: form.phone, password: form.password,
        firstName: form.firstName || undefined, lastName: form.lastName || undefined,
      }),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ["org-members"] });
      setDialogRole(null);
      toast({ title: "Xodim qo'shildi" });
    },
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
      toast({ title: "O'qituvchi qo'shildi" });
    },
  });

  const deactivateMutation = useMutation({
    mutationFn: (id: string) => membersApi.deactivate(id),
    onSuccess: () => qc.invalidateQueries({ queryKey: ["org-members"] }),
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

  return (
    <div className="max-w-4xl mx-auto space-y-6">
      <div className="flex items-center justify-between flex-wrap gap-3">
        <div>
          <h1 className="text-2xl font-bold">Xodimlar</h1>
          <p className="text-muted-foreground mt-1">Jami: {data?.length ?? 0} ta</p>
        </div>
        <div className="flex gap-2">
          {isOrgAdmin && (
            <Button variant="outline" onClick={() => openCreate("Staff")}>
              <Plus className="h-4 w-4 mr-2" />
              Xodim qo'shish
            </Button>
          )}
          <Button onClick={() => openCreate("Teacher")}>
            <Plus className="h-4 w-4 mr-2" />
            O'qituvchi qo'shish
          </Button>
        </div>
      </div>

      <Card>
        <CardContent className="p-0">
          <Table>
            <TableHeader>
              <TableRow>
                <TableHead>Ism</TableHead>
                <TableHead>Telefon</TableHead>
                <TableHead>Rol</TableHead>
                <TableHead>Holat</TableHead>
                {isOrgAdmin && <TableHead className="text-right w-20">Amallar</TableHead>}
              </TableRow>
            </TableHeader>
            <TableBody>
              {data?.map((m: MemberDto) => (
                <TableRow key={m.id}>
                  <TableCell className="font-medium">{m.fullName?.trim() || "—"}</TableCell>
                  <TableCell className="font-mono text-sm">{m.phone ?? "—"}</TableCell>
                  <TableCell>
                    <Badge variant="secondary" className="text-xs">{ROLE_LABELS[m.role] ?? m.role}</Badge>
                  </TableCell>
                  <TableCell>
                    <Badge variant={m.isActive ? "success" : "secondary"} className="text-xs">
                      {m.isActive ? "Faol" : "Nofaol"}
                    </Badge>
                  </TableCell>
                  {isOrgAdmin && (
                    <TableCell className="text-right">
                      {m.isActive && (
                        <Button variant="ghost" size="icon" onClick={() => deactivateMutation.mutate(m.id)} title="Deaktivatsiya">
                          <UserX className="h-4 w-4 text-red-500" />
                        </Button>
                      )}
                    </TableCell>
                  )}
                </TableRow>
              ))}
              {data?.length === 0 && (
                <TableRow>
                  <TableCell colSpan={5} className="text-center text-muted-foreground py-10">
                    Xodimlar yo'q
                  </TableCell>
                </TableRow>
              )}
            </TableBody>
          </Table>
        </CardContent>
      </Card>

      <Dialog open={dialogRole !== null} onOpenChange={(o) => !o && setDialogRole(null)}>
        <DialogContent>
          <DialogHeader>
            <DialogTitle>{dialogRole === "Staff" ? "Yangi xodim" : "Yangi o'qituvchi"}</DialogTitle>
          </DialogHeader>
          <div className="space-y-4 py-2">
            <div className="space-y-1.5">
              <Label>Ism</Label>
              <Input value={form.firstName} onChange={(e) => setForm({ ...form, firstName: e.target.value })} />
            </div>
            <div className="space-y-1.5">
              <Label>Familiya</Label>
              <Input value={form.lastName} onChange={(e) => setForm({ ...form, lastName: e.target.value })} />
            </div>
            <div className="space-y-1.5">
              <Label>Telefon (login)</Label>
              <Input value={form.phone} onChange={(e) => setForm({ ...form, phone: e.target.value })} placeholder="+998 XX XXX XX XX" />
            </div>
            <div className="space-y-1.5">
              <Label>Parol</Label>
              <Input type="text" value={form.password} onChange={(e) => setForm({ ...form, password: e.target.value })} placeholder="Kamida 6 ta belgi" />
            </div>
          </div>
          <DialogFooter>
            <Button variant="outline" onClick={() => setDialogRole(null)}>Bekor</Button>
            <Button
              onClick={handleSubmit}
              disabled={!form.phone.trim() || form.password.length < 6 || submitting}
            >
              Saqlash
            </Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>
    </div>
  );
}
