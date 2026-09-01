import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import { useState } from "react";
import { useParams, useNavigate } from "react-router-dom";
import { studentsApi } from "@/api/students";
import { groupsApi } from "@/api/groups";
import { financeApi } from "@/api/finance";
import { useBranchStore } from "@/store/branch";
import { Card, CardContent } from "@/components/ui/card";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Textarea } from "@/components/ui/textarea";
import { Badge } from "@/components/ui/badge";
import { Avatar, AvatarFallback } from "@/components/ui/avatar";
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from "@/components/ui/table";
import { Dialog, DialogContent, DialogHeader, DialogTitle, DialogFooter } from "@/components/ui/dialog";
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "@/components/ui/select";
import { Tabs, TabsList, TabsTrigger, TabsContent } from "@/components/ui/tabs";
import { PageLoader } from "@/components/shared/LoadingSpinner";
import { MonthYearPicker } from "@/components/shared/MonthYearPicker";
import { toast } from "@/components/ui/use-toast";
import {
  ArrowLeft, KeyRound, Wallet, UserPlus, Layers, CalendarCheck2,
  Building2, CalendarClock, StickyNote, Pencil, ChevronLeft, ChevronRight, Check, X,
} from "lucide-react";
import { MEMBERSHIP_STATUS_LABELS, PAYMENT_METHOD_LABELS, PAYMENT_METHODS, formatMonthLabel, formatPhone } from "@/lib/groupHelpers";
import type { PaymentMethod, StudentDetailsDto } from "@/types";

type EditFormState = { fullName: string; phone: string; email: string; notes: string };

const MONTH_NAMES = [
  "Yanvar", "Fevral", "Mart", "Aprel", "May", "Iyun",
  "Iyul", "Avgust", "Sentyabr", "Oktyabr", "Noyabr", "Dekabr",
];

function StatTile({ icon: Icon, label, value, tone }: { icon: React.ElementType; label: string; value: string; tone?: "positive" | "negative" }) {
  return (
    <div className="flex items-center gap-3 rounded-lg border bg-muted/30 px-4 py-3">
      <div className="flex h-9 w-9 items-center justify-center rounded-md bg-background border flex-shrink-0">
        <Icon className="h-4 w-4 text-muted-foreground" />
      </div>
      <div className="min-w-0">
        <div className={`text-sm font-semibold truncate ${tone === "negative" ? "text-destructive" : tone === "positive" ? "text-emerald-500" : ""}`}>
          {value}
        </div>
        <div className="text-xs text-muted-foreground">{label}</div>
      </div>
    </div>
  );
}

function initials(fullName: string) {
  return fullName
    .split(" ")
    .filter(Boolean)
    .slice(0, 2)
    .map((p) => p[0]?.toUpperCase())
    .join("");
}

function formatDate(iso: string): string {
  const d = new Date(iso);
  return `${String(d.getDate()).padStart(2, "0")}.${String(d.getMonth() + 1).padStart(2, "0")}.${d.getFullYear()}`;
}

function NotesSection({ student, onSave, isPending }: { student: StudentDetailsDto; onSave: (notes: string) => void; isPending: boolean }) {
  const [editing, setEditing] = useState(false);
  const [value, setValue] = useState(student.notes ?? "");

  return (
    <Card>
      <CardContent className="p-5 space-y-3">
        <div className="flex items-center justify-between">
          <span className="text-sm font-semibold flex items-center gap-2">
            <StickyNote className="h-4 w-4 text-muted-foreground" />
            Izohlar
          </span>
          {!editing && (
            <Button variant="ghost" size="sm" onClick={() => { setValue(student.notes ?? ""); setEditing(true); }}>
              <Pencil className="h-3.5 w-3.5 mr-1.5" />
              Tahrirlash
            </Button>
          )}
        </div>
        {editing ? (
          <div className="space-y-2">
            <Textarea value={value} onChange={(e) => setValue(e.target.value)} rows={4} placeholder="Izoh qoldiring..." />
            <div className="flex justify-end gap-2">
              <Button variant="outline" size="sm" onClick={() => setEditing(false)}>Bekor</Button>
              <Button size="sm" disabled={isPending} onClick={() => { onSave(value); setEditing(false); }}>Saqlash</Button>
            </div>
          </div>
        ) : (
          <p className="text-sm text-muted-foreground whitespace-pre-wrap">
            {student.notes || "Izoh yo'q"}
          </p>
        )}
      </CardContent>
    </Card>
  );
}

function StudentAttendanceTab({ studentId }: { studentId: string }) {
  const now = new Date();
  const [year, setYear] = useState(now.getFullYear());
  const [month, setMonth] = useState(now.getMonth() + 1);

  const { data, isLoading } = useQuery({
    queryKey: ["student-attendance", studentId, year, month],
    queryFn: () => studentsApi.getAttendance(studentId, year, month),
  });

  const shiftMonth = (delta: number) => {
    let m = month + delta;
    let y = year;
    if (m < 1) { m = 12; y -= 1; }
    if (m > 12) { m = 1; y += 1; }
    setYear(y);
    setMonth(m);
  };

  return (
    <div className="space-y-4">
      <div className="flex items-center justify-between">
        <Button variant="outline" size="icon" onClick={() => shiftMonth(-1)}>
          <ChevronLeft className="h-4 w-4" />
        </Button>
        <span className="text-sm font-medium">{MONTH_NAMES[month - 1]} {year}</span>
        <Button variant="outline" size="icon" onClick={() => shiftMonth(1)}>
          <ChevronRight className="h-4 w-4" />
        </Button>
      </div>

      {isLoading || !data ? (
        <PageLoader />
      ) : data.groups.length === 0 ? (
        <p className="text-sm text-muted-foreground text-center py-10">Guruhlar yo'q</p>
      ) : (
        data.groups.map((g) => (
          <Card key={g.groupId}>
            <CardContent className="p-4 space-y-3">
              <div className="flex items-center justify-between">
                <span className="font-medium text-sm">{g.groupName}</span>
                <span className="text-xs text-muted-foreground">{g.presentCount} keldi / {g.absentCount} kelmadi</span>
              </div>
              {g.lessonDates.length === 0 ? (
                <p className="text-sm text-muted-foreground text-center py-4">Bu oyda darslar yo'q</p>
              ) : (
                <div className="overflow-x-auto">
                  <div className="flex gap-1.5">
                    {g.lessonDates.map((d) => {
                      const status = g.marks[d];
                      return (
                        <div key={d} className="flex flex-col items-center gap-1 flex-shrink-0 w-9">
                          <span className="text-[10px] text-muted-foreground">{d.slice(8, 10)}</span>
                          <div className={`h-7 w-7 rounded flex items-center justify-center ${
                            status === "Present" ? "bg-emerald-500/15" : status === "Absent" ? "bg-red-500/15" : "border border-dashed border-muted-foreground/30"
                          }`}>
                            {status === "Present" && <Check className="h-3.5 w-3.5 text-emerald-500" />}
                            {status === "Absent" && <X className="h-3.5 w-3.5 text-red-500" />}
                          </div>
                        </div>
                      );
                    })}
                  </div>
                </div>
              )}
            </CardContent>
          </Card>
        ))
      )}
    </div>
  );
}

export function StudentProfilePage() {
  const { id: studentId } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const qc = useQueryClient();
  const branchId = useBranchStore((s) => s.branchId);

  const [tab, setTab] = useState<"groups" | "attendance" | "payments">("groups");
  const [editOpen, setEditOpen] = useState(false);
  const [editForm, setEditForm] = useState<EditFormState>({ fullName: "", phone: "", email: "", notes: "" });
  const [loginPassword, setLoginPassword] = useState("");
  const [loginOpen, setLoginOpen] = useState(false);
  const [addGroupOpen, setAddGroupOpen] = useState(false);
  const [addGroupId, setAddGroupId] = useState("");
  const [paymentOpen, setPaymentOpen] = useState(false);
  const [paymentGroupId, setPaymentGroupId] = useState("");
  const [paymentAmount, setPaymentAmount] = useState("");
  const [paymentDate, setPaymentDate] = useState(new Date().toISOString().slice(0, 10));
  const [paymentForMonth, setPaymentForMonth] = useState(new Date().toISOString().slice(0, 7));
  const [paymentMethod, setPaymentMethod] = useState<PaymentMethod>("Cash");

  const { data: student, isLoading } = useQuery({
    queryKey: ["crm-student", studentId],
    queryFn: () => studentsApi.getById(studentId!),
    enabled: !!studentId,
  });

  const { data: payments } = useQuery({
    queryKey: ["finance-payments", "student", studentId],
    queryFn: () => financeApi.getPayments({ studentId: studentId!, pageSize: 100 }),
    enabled: !!studentId && tab === "payments",
  });

  const { data: allGroups } = useQuery({
    queryKey: ["crm-groups", branchId, "for-add"],
    queryFn: () => groupsApi.list({ branchId: branchId ?? undefined, isActive: true, pageSize: 200 }),
    enabled: addGroupOpen,
  });

  const invalidate = () => qc.invalidateQueries({ queryKey: ["crm-student", studentId] });

  const updateMutation = useMutation({
    mutationFn: () => studentsApi.update(studentId!, {
      fullName: editForm.fullName, phone: editForm.phone,
      email: editForm.email || undefined, notes: editForm.notes || undefined,
    }),
    onSuccess: () => { invalidate(); setEditOpen(false); toast({ title: "Talaba yangilandi" }); },
  });

  const updateNotesMutation = useMutation({
    mutationFn: (notes: string) => studentsApi.update(studentId!, {
      fullName: student!.fullName, phone: student!.phone, email: student!.email, notes: notes || undefined,
    }),
    onSuccess: () => { invalidate(); toast({ title: "Izoh saqlandi" }); },
  });

  const createLoginMutation = useMutation({
    mutationFn: () => studentsApi.createLogin(studentId!, loginPassword),
    onSuccess: () => { invalidate(); setLoginOpen(false); setLoginPassword(""); toast({ title: "Login yaratildi" }); },
  });

  const addToGroupMutation = useMutation({
    mutationFn: () => groupsApi.addStudent(addGroupId, studentId!),
    onSuccess: () => {
      invalidate();
      setAddGroupOpen(false);
      setAddGroupId("");
      toast({ title: "Guruhga qo'shildi" });
    },
    onError: (e: unknown) => {
      const msg = (e as any)?.response?.data?.detail;
      toast({ title: "Xatolik", description: msg ?? "Guruhga qo'shib bo'lmadi", variant: "destructive" });
    },
  });

  const recordPaymentMutation = useMutation({
    mutationFn: () => financeApi.recordPayment({
      groupId: paymentGroupId, studentId: studentId!,
      amount: Number(paymentAmount), paidAt: paymentDate, forMonth: `${paymentForMonth}-01`, method: paymentMethod,
    }),
    onSuccess: () => {
      invalidate();
      qc.invalidateQueries({ queryKey: ["finance-payments", "student", studentId] });
      setPaymentOpen(false);
      setPaymentAmount("");
      toast({ title: "To'lov qabul qilindi" });
    },
    onError: () => {
      toast({ title: "Xatolik", description: "To'lovni saqlab bo'lmadi", variant: "destructive" });
    },
  });

  if (isLoading || !student) return <PageLoader />;

  const openEdit = () => {
    setEditForm({ fullName: student.fullName, phone: student.phone, email: student.email ?? "", notes: student.notes ?? "" });
    setEditOpen(true);
  };

  const groupsNotJoined = (allGroups?.items ?? []).filter(
    (g) => !student.groups.some((sg) => sg.groupId === g.id)
  );

  return (
    <div className="max-w-4xl mx-auto space-y-6">
      <Button variant="ghost" size="sm" onClick={() => navigate("/crm/students")} className="-ml-2">
        <ArrowLeft className="h-4 w-4 mr-1.5" />
        Talabalar
      </Button>

      <Card>
        <CardContent className="p-6 space-y-5">
          <div className="flex items-start justify-between flex-wrap gap-3">
            <div className="flex items-center gap-4">
              <Avatar className="h-14 w-14 border">
                <AvatarFallback className="text-base font-semibold">{initials(student.fullName)}</AvatarFallback>
              </Avatar>
              <div>
                <h1 className="text-2xl font-bold">{student.fullName}</h1>
                <p className="text-muted-foreground text-sm mt-0.5">{formatPhone(student.phone)}</p>
                <div className="flex flex-wrap gap-x-4 gap-y-0.5 mt-1.5">
                  <p className="text-xs text-muted-foreground flex items-center gap-1">
                    <Building2 className="h-3 w-3" />
                    {student.branchName}
                  </p>
                  <p className="text-xs text-muted-foreground flex items-center gap-1">
                    <CalendarClock className="h-3 w-3" />
                    {student.startedAt ? `O'qishni boshlagan: ${formatDate(student.startedAt)}` : "Hali faollashtirilmagan"}
                  </p>
                </div>
                {student.leadId && (
                  <p className="text-xs text-muted-foreground mt-1">
                    Lid orqali qo'shilgan: {student.leadFullName}
                  </p>
                )}
              </div>
            </div>
          </div>

          <div className="grid grid-cols-2 sm:grid-cols-3 gap-3">
            <StatTile
              icon={Wallet}
              label="Balans"
              value={`${student.totalBalance.toLocaleString()} so'm`}
              tone={student.totalBalance < 0 ? "negative" : "positive"}
            />
            <StatTile icon={Layers} label="Guruhlar" value={`${student.groups.length} ta`} />
            <StatTile
              icon={CalendarCheck2}
              label="Davomat"
              value={`${student.groups.reduce((n, g) => n + g.presentCount, 0)} / ${student.groups.reduce((n, g) => n + g.presentCount + g.absentCount, 0)}`}
            />
          </div>

          <div className="flex flex-wrap gap-2 pt-1 border-t border-transparent">
            <Button variant="outline" size="sm" onClick={openEdit}>Tahrirlash</Button>
            <Button variant="outline" size="sm" onClick={() => setAddGroupOpen(true)}>
              <UserPlus className="h-4 w-4 mr-1.5" />
              Guruhga qo'shish
            </Button>
            <Button variant="outline" size="sm" onClick={() => setPaymentOpen(true)} disabled={student.groups.length === 0}>
              <Wallet className="h-4 w-4 mr-1.5" />
              To'lov qo'shish
            </Button>
            {!student.userId && (
              <Button variant="outline" size="sm" onClick={() => setLoginOpen(true)}>
                <KeyRound className="h-4 w-4 mr-1.5" />
                Login yaratish
              </Button>
            )}
          </div>
        </CardContent>
      </Card>

      <Tabs value={tab} onValueChange={(v) => setTab(v as "groups" | "attendance" | "payments")}>
        <TabsList>
          <TabsTrigger value="groups">Guruhlar</TabsTrigger>
          <TabsTrigger value="attendance">Davomat</TabsTrigger>
          <TabsTrigger value="payments">To'lovlar</TabsTrigger>
        </TabsList>

        <TabsContent value="groups" className="space-y-2.5 mt-4">
          {student.groups.length === 0 && (
            <p className="text-sm text-muted-foreground text-center py-10">Guruhlar yo'q</p>
          )}
          {student.groups.map((g) => (
            <Card
              key={g.groupId}
              className="cursor-pointer hover:border-foreground/20 hover:shadow-sm transition-all"
              onClick={() => navigate(`/crm/groups/${g.groupId}`)}
            >
              <CardContent className="p-4 flex items-center justify-between flex-wrap gap-3">
                <div className="flex items-center gap-3 min-w-0">
                  <div className="flex h-9 w-9 items-center justify-center rounded-md bg-muted border flex-shrink-0">
                    <Layers className="h-4 w-4 text-muted-foreground" />
                  </div>
                  <div className="min-w-0">
                    <div className="font-medium flex items-center gap-2 flex-wrap">
                      {g.groupName}
                      <Badge variant="secondary" className="text-xs">{MEMBERSHIP_STATUS_LABELS[g.status]}</Badge>
                    </div>
                    <div className="text-xs text-muted-foreground mt-0.5">
                      {g.teacherName ?? "O'qituvchi yo'q"} · {g.presentCount} keldi / {g.absentCount} kelmadi
                    </div>
                  </div>
                </div>
                <span className={`text-sm font-semibold flex-shrink-0 ${g.balance < 0 ? "text-destructive" : "text-emerald-500"}`}>
                  {g.balance.toLocaleString()} so'm
                </span>
              </CardContent>
            </Card>
          ))}
        </TabsContent>

        <TabsContent value="attendance" className="mt-4">
          <StudentAttendanceTab studentId={studentId!} />
        </TabsContent>

        <TabsContent value="payments" className="mt-4">
          <Card>
            <CardContent className="p-0">
              <Table>
                <TableHeader>
                  <TableRow>
                    <TableHead>Guruh</TableHead>
                    <TableHead>Summa</TableHead>
                    <TableHead>Qaysi oy uchun</TableHead>
                    <TableHead>Usul</TableHead>
                    <TableHead>Sana</TableHead>
                  </TableRow>
                </TableHeader>
                <TableBody>
                  {payments?.items.map((p) => (
                    <TableRow key={p.id}>
                      <TableCell className="text-sm">{p.groupName}</TableCell>
                      <TableCell className="text-sm">{p.amount.toLocaleString()} so'm</TableCell>
                      <TableCell className="text-sm">{formatMonthLabel(p.forMonth)}</TableCell>
                      <TableCell className="text-sm">
                        <Badge variant="outline" className="text-xs">{PAYMENT_METHOD_LABELS[p.method]}</Badge>
                      </TableCell>
                      <TableCell className="text-sm">{p.paidAt}</TableCell>
                    </TableRow>
                  ))}
                  {payments?.items.length === 0 && (
                    <TableRow>
                      <TableCell colSpan={5} className="text-center text-muted-foreground py-10">
                        To'lovlar yo'q
                      </TableCell>
                    </TableRow>
                  )}
                </TableBody>
              </Table>
            </CardContent>
          </Card>
        </TabsContent>
      </Tabs>

      <NotesSection
        student={student}
        isPending={updateNotesMutation.isPending}
        onSave={(notes) => updateNotesMutation.mutate(notes)}
      />

      {/* Edit dialog */}
      <Dialog open={editOpen} onOpenChange={setEditOpen}>
        <DialogContent>
          <DialogHeader>
            <DialogTitle>Talabani tahrirlash</DialogTitle>
          </DialogHeader>
          <div className="space-y-4 py-2">
            <div className="space-y-1.5">
              <Label>Ism familiya</Label>
              <Input value={editForm.fullName} onChange={(e) => setEditForm({ ...editForm, fullName: e.target.value })} />
            </div>
            <div className="space-y-1.5">
              <Label>Telefon</Label>
              <Input value={editForm.phone} onChange={(e) => setEditForm({ ...editForm, phone: e.target.value })} />
            </div>
            <div className="space-y-1.5">
              <Label>Email</Label>
              <Input type="email" value={editForm.email} onChange={(e) => setEditForm({ ...editForm, email: e.target.value })} />
            </div>
            <div className="space-y-1.5">
              <Label>Izoh</Label>
              <Textarea value={editForm.notes} onChange={(e) => setEditForm({ ...editForm, notes: e.target.value })} />
            </div>
          </div>
          <DialogFooter>
            <Button variant="outline" onClick={() => setEditOpen(false)}>Bekor</Button>
            <Button
              onClick={() => updateMutation.mutate()}
              disabled={!editForm.fullName.trim() || !editForm.phone.trim() || updateMutation.isPending}
            >
              Saqlash
            </Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>

      {/* Login dialog */}
      <Dialog open={loginOpen} onOpenChange={setLoginOpen}>
        <DialogContent>
          <DialogHeader>
            <DialogTitle>Login yaratish</DialogTitle>
          </DialogHeader>
          <div className="space-y-4 py-2">
            <div className="space-y-1.5">
              <Label>Login (telefon)</Label>
              <Input value={student.phone} disabled />
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
            <Button variant="outline" onClick={() => setLoginOpen(false)}>Bekor</Button>
            <Button
              onClick={() => createLoginMutation.mutate()}
              disabled={loginPassword.length < 6 || createLoginMutation.isPending}
            >
              Yaratish
            </Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>

      {/* Add to group dialog */}
      <Dialog open={addGroupOpen} onOpenChange={setAddGroupOpen}>
        <DialogContent>
          <DialogHeader>
            <DialogTitle>Guruhga qo'shish</DialogTitle>
          </DialogHeader>
          <div className="py-2">
            <Select value={addGroupId} onValueChange={setAddGroupId}>
              <SelectTrigger>
                <SelectValue placeholder="Guruh tanlang" />
              </SelectTrigger>
              <SelectContent>
                {groupsNotJoined.map((g) => (
                  <SelectItem key={g.id} value={g.id}>{g.name}</SelectItem>
                ))}
              </SelectContent>
            </Select>
          </div>
          <DialogFooter>
            <Button variant="outline" onClick={() => setAddGroupOpen(false)}>Bekor</Button>
            <Button
              disabled={!addGroupId || addToGroupMutation.isPending}
              onClick={() => addToGroupMutation.mutate()}
            >
              Qo'shish
            </Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>

      {/* Payment dialog */}
      <Dialog open={paymentOpen} onOpenChange={setPaymentOpen}>
        <DialogContent>
          <DialogHeader>
            <DialogTitle>To'lov qo'shish</DialogTitle>
          </DialogHeader>
          <div className="space-y-4 py-2">
            <div className="space-y-1.5">
              <Label>Guruh</Label>
              <Select
                value={paymentGroupId}
                onValueChange={(v) => {
                  setPaymentGroupId(v);
                  const g = student.groups.find((sg) => sg.groupId === v);
                  if (g) {
                    setPaymentAmount(String(g.effectivePrice));
                    setPaymentForMonth((g.nextPaymentDueDate ?? new Date().toISOString().slice(0, 10)).slice(0, 7));
                  }
                }}
              >
                <SelectTrigger>
                  <SelectValue placeholder="Guruh tanlang" />
                </SelectTrigger>
                <SelectContent>
                  {student.groups.map((g) => (
                    <SelectItem key={g.groupId} value={g.groupId}>{g.groupName}</SelectItem>
                  ))}
                </SelectContent>
              </Select>
            </div>
            <div className="space-y-1.5">
              <Label>Summa (so'm)</Label>
              <Input type="number" min="0" value={paymentAmount} onChange={(e) => setPaymentAmount(e.target.value)} />
            </div>
            <div className="space-y-1.5">
              <Label>Qaysi oy uchun</Label>
              <MonthYearPicker value={paymentForMonth} onChange={setPaymentForMonth} />
            </div>
            <div className="space-y-1.5">
              <Label>Sana</Label>
              <Input type="date" value={paymentDate} onChange={(e) => setPaymentDate(e.target.value)} />
            </div>
            <div className="space-y-1.5">
              <Label>To'lov usuli</Label>
              <Select value={paymentMethod} onValueChange={(v) => setPaymentMethod(v as PaymentMethod)}>
                <SelectTrigger>
                  <SelectValue />
                </SelectTrigger>
                <SelectContent>
                  {PAYMENT_METHODS.map((m) => (
                    <SelectItem key={m} value={m}>{PAYMENT_METHOD_LABELS[m]}</SelectItem>
                  ))}
                </SelectContent>
              </Select>
            </div>
          </div>
          <DialogFooter>
            <Button variant="outline" onClick={() => setPaymentOpen(false)}>Bekor</Button>
            <Button
              disabled={!paymentGroupId || !paymentAmount || Number(paymentAmount) <= 0 || !paymentForMonth || recordPaymentMutation.isPending}
              onClick={() => recordPaymentMutation.mutate()}
            >
              Saqlash
            </Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>
    </div>
  );
}
