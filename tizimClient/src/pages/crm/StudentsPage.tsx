import { useQuery, useMutation, useQueryClient, keepPreviousData } from "@tanstack/react-query";
import { useState, useEffect } from "react";
import { useNavigate } from "react-router-dom";
import { studentsApi } from "@/api/students";
import { financeApi } from "@/api/finance";
import { groupsApi } from "@/api/groups";
import { useBranchStore } from "@/store/branch";
import { Card, CardContent } from "@/components/ui/card";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Textarea } from "@/components/ui/textarea";
import { Badge } from "@/components/ui/badge";
import { Switch } from "@/components/ui/switch";
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "@/components/ui/select";
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from "@/components/ui/table";
import { Dialog, DialogContent, DialogHeader, DialogTitle, DialogFooter } from "@/components/ui/dialog";
import { PageLoader } from "@/components/shared/LoadingSpinner";
import { toast } from "@/components/ui/use-toast";
import { Plus, Pencil, Search, KeyRound } from "lucide-react";
import { formatPhone } from "@/lib/groupHelpers";
import type { StudentDto } from "@/types";

const FINANCIAL_STATUS_OPTIONS = [
  { value: "WithDebt", label: "Qarzdor" },
  { value: "WithDiscount", label: "Chegirmali" },
  { value: "WithoutDebt", label: "Qarzi yo'q" },
  { value: "PositiveBalance", label: "Musbat balans" },
  { value: "PaidThisMonth", label: "Shu oyda to'lagan" },
];

const STUDENT_STATUS_OPTIONS = [
  { value: "AddedThisMonth", label: "Shu oyda qo'shilgan" },
  { value: "Trial", label: "Sinov darsida" },
  { value: "Active", label: "Faol" },
  { value: "Frozen", label: "Muzlatilgan" },
  { value: "WithoutGroup", label: "Guruhsiz" },
  { value: "LeftAfterTrial", label: "Sinovdan keyin ketgan" },
];

type FormState = { fullName: string; phone: string; email: string; notes: string };
const emptyForm: FormState = { fullName: "", phone: "", email: "", notes: "" };

export function StudentsPage() {
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

  useEffect(() => {
    const t = setTimeout(() => setSearch(searchInput), 250);
    return () => clearTimeout(t);
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

  const { data: debtors } = useQuery({
    queryKey: ["finance-debtors"],
    queryFn: () => financeApi.getDebtors(),
  });
  const debtorStudentIds = new Set(debtors?.map((d) => d.studentId));

  const createMutation = useMutation({
    mutationFn: () =>
      studentsApi.create({ branchId: branchId!, fullName: form.fullName, phone: form.phone, email: form.email || undefined }),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ["crm-students"] });
      setDialogOpen(false);
      toast({ title: "Talaba qo'shildi" });
    },
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
      toast({ title: "Talaba yangilandi" });
    },
  });

  const toggleMutation = useMutation({
    mutationFn: ({ id, isActive }: { id: string; isActive: boolean }) => studentsApi.toggleActive(id, isActive),
    onSuccess: () => qc.invalidateQueries({ queryKey: ["crm-students"] }),
  });

  const createLoginMutation = useMutation({
    mutationFn: () => studentsApi.createLogin(loginStudent!.id, loginPassword),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ["crm-students"] });
      setLoginStudent(null);
      setLoginPassword("");
      toast({ title: "Login yaratildi" });
    },
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

  return (
    <div className="max-w-5xl mx-auto space-y-6">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-2xl font-bold">Talabalar</h1>
          <p className="text-muted-foreground mt-1">Jami: {data?.totalCount ?? 0} ta</p>
        </div>
        <Button onClick={openCreate} disabled={!branchId}>
          <Plus className="h-4 w-4 mr-2" />
          Talaba qo'shish
        </Button>
      </div>

      <div className="flex flex-wrap gap-2">
        <div className="relative max-w-sm flex-1 min-w-[200px]">
          <Search className="h-4 w-4 absolute left-2.5 top-1/2 -translate-y-1/2 text-muted-foreground" />
          <Input
            placeholder="Ism yoki telefon bo'yicha qidirish..."
            value={searchInput}
            onChange={(e) => setSearchInput(e.target.value)}
            className="pl-8"
          />
        </div>

        <Select value={groupFilter} onValueChange={setGroupFilter}>
          <SelectTrigger className="w-44">
            <SelectValue placeholder="Guruh" />
          </SelectTrigger>
          <SelectContent>
            <SelectItem value="all">Barcha guruhlar</SelectItem>
            {groupOptions?.items.map((g) => (
              <SelectItem key={g.id} value={g.id}>{g.name}</SelectItem>
            ))}
          </SelectContent>
        </Select>

        <Select value={financialFilter} onValueChange={setFinancialFilter}>
          <SelectTrigger className="w-48">
            <SelectValue placeholder="Moliyaviy holat" />
          </SelectTrigger>
          <SelectContent>
            <SelectItem value="all">Barcha moliyaviy holatlar</SelectItem>
            {FINANCIAL_STATUS_OPTIONS.map((o) => (
              <SelectItem key={o.value} value={o.value}>{o.label}</SelectItem>
            ))}
          </SelectContent>
        </Select>

        <Select value={studentStatusFilter} onValueChange={setStudentStatusFilter}>
          <SelectTrigger className="w-48">
            <SelectValue placeholder="Talaba holati" />
          </SelectTrigger>
          <SelectContent>
            <SelectItem value="all">Barcha holatlar</SelectItem>
            {STUDENT_STATUS_OPTIONS.map((o) => (
              <SelectItem key={o.value} value={o.value}>{o.label}</SelectItem>
            ))}
          </SelectContent>
        </Select>
      </div>

      <Card>
        <CardContent className="p-0">
          <Table>
            <TableHeader>
              <TableRow>
                <TableHead>Ism</TableHead>
                <TableHead>Telefon</TableHead>
                <TableHead>Holat</TableHead>
                <TableHead>Qo'shilgan</TableHead>
                <TableHead className="text-right w-24">Amallar</TableHead>
              </TableRow>
            </TableHeader>
            <TableBody>
              {data?.items.map((student) => (
                <TableRow key={student.id} className="cursor-pointer" onClick={() => navigate(`/crm/students/${student.id}`)}>
                  <TableCell className="font-medium">
                    {student.fullName}
                    {student.leadId && <Badge variant="secondary" className="text-xs ml-2">lid orqali</Badge>}
                    {debtorStudentIds.has(student.id) && <Badge variant="destructive" className="text-xs ml-2">Qarzdor</Badge>}
                  </TableCell>
                  <TableCell className="font-mono text-base font-medium tracking-wide">{formatPhone(student.phone)}</TableCell>
                  <TableCell onClick={(e) => e.stopPropagation()}>
                    <div className="flex items-center gap-2">
                      <Switch
                        checked={student.isActive}
                        onCheckedChange={(checked) => toggleMutation.mutate({ id: student.id, isActive: checked })}
                      />
                      <Badge variant={student.isActive ? "success" : "secondary"} className="text-xs">
                        {student.isActive ? "O'qiyapti" : "Ketgan"}
                      </Badge>
                    </div>
                  </TableCell>
                  <TableCell className="text-sm text-muted-foreground">
                    {new Date(student.createdAt).toLocaleDateString("ru-RU")}
                  </TableCell>
                  <TableCell className="text-right" onClick={(e) => e.stopPropagation()}>
                    {student.userId ? (
                      <Badge variant="success" className="text-xs mr-1">Login bor</Badge>
                    ) : (
                      <Button
                        variant="ghost"
                        size="icon"
                        title="Login yaratish"
                        onClick={() => { setLoginStudent(student); setLoginPassword(""); }}
                      >
                        <KeyRound className="h-4 w-4 text-cyan-500" />
                      </Button>
                    )}
                    <Button variant="ghost" size="icon" onClick={() => openEdit(student)}>
                      <Pencil className="h-4 w-4" />
                    </Button>
                  </TableCell>
                </TableRow>
              ))}
              {data?.items.length === 0 && (
                <TableRow>
                  <TableCell colSpan={5} className="text-center text-muted-foreground py-10">
                    Talabalar topilmadi
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
            <DialogTitle>{editing ? "Talabani tahrirlash" : "Yangi talaba"}</DialogTitle>
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
            <div className="space-y-1.5">
              <Label>Email</Label>
              <Input type="email" value={form.email} onChange={(e) => setForm({ ...form, email: e.target.value })} />
            </div>
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

      <Dialog open={!!loginStudent} onOpenChange={(o) => !o && setLoginStudent(null)}>
        <DialogContent>
          <DialogHeader>
            <DialogTitle>Login yaratish</DialogTitle>
          </DialogHeader>
          <div className="space-y-4 py-2">
            <div className="space-y-1.5">
              <Label>Login (telefon)</Label>
              <Input value={loginStudent?.phone ?? ""} disabled />
            </div>
            <div className="space-y-1.5">
              <Label>Parol</Label>
              <Input
                type="text"
                value={loginPassword}
                onChange={(e) => setLoginPassword(e.target.value)}
                placeholder="Kamida 6 ta belgi"
              />
            </div>
          </div>
          <DialogFooter>
            <Button variant="outline" onClick={() => setLoginStudent(null)}>Bekor</Button>
            <Button
              onClick={() => createLoginMutation.mutate()}
              disabled={loginPassword.length < 6 || createLoginMutation.isPending}
            >
              Yaratish
            </Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>
    </div>
  );
}
