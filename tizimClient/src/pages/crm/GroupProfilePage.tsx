import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import { useState } from "react";
import { useParams, useNavigate } from "react-router-dom";
import { groupsApi } from "@/api/groups";
import { studentsApi } from "@/api/students";
import { membersApi } from "@/api/members";
import { roomsApi } from "@/api/rooms";
import { financeApi } from "@/api/finance";
import { Card, CardContent } from "@/components/ui/card";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Badge } from "@/components/ui/badge";
import { Separator } from "@/components/ui/separator";
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "@/components/ui/select";
import { Tabs, TabsList, TabsTrigger, TabsContent } from "@/components/ui/tabs";
import {
  DropdownMenu, DropdownMenuContent, DropdownMenuItem,
  DropdownMenuSeparator, DropdownMenuTrigger,
} from "@/components/ui/dropdown-menu";
import { PageLoader } from "@/components/shared/LoadingSpinner";
import { AttendanceDialog } from "@/components/shared/AttendanceDialog";
import { RecordPaymentDialog, DiscountDialog } from "@/components/shared/GroupStudentActionDialogs";
import { toast } from "@/components/ui/use-toast";
import {
  ArrowLeft, Plus, Trash2, Wallet, Snowflake, Percent, MoreVertical, Users,
} from "lucide-react";
import {
  MEMBERSHIP_STATUS_LABELS, membershipBadgeVariant,
  DAYS, DAY_LABELS, ODD_DAYS, EVEN_DAYS, withSeconds, addHours,
} from "@/lib/groupHelpers";
import type { GroupScheduleSlotDto, GroupMembershipStatus, GroupStudentDto, DayOfWeek } from "@/types";

function InfoRow({ label, children }: { label: string; children: React.ReactNode }) {
  return (
    <div className="space-y-1.5">
      <Label className="text-xs text-muted-foreground">{label}</Label>
      {children}
    </div>
  );
}

export function GroupProfilePage() {
  const { id: groupId } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const qc = useQueryClient();

  const [addStudentId, setAddStudentId] = useState<string>("");
  const [scheduleDraft, setScheduleDraft] = useState<GroupScheduleSlotDto[] | null>(null);
  const [presetTime, setPresetTime] = useState("14:00");
  const [paymentStudent, setPaymentStudent] = useState<GroupStudentDto | null>(null);
  const [discountStudent, setDiscountStudent] = useState<GroupStudentDto | null>(null);

  const { data: group, isLoading } = useQuery({
    queryKey: ["crm-group", groupId],
    queryFn: () => groupsApi.getById(groupId!),
    enabled: !!groupId,
  });

  const { data: teachers } = useQuery({
    queryKey: ["members-teachers"],
    queryFn: () => membersApi.list("Teacher"),
  });

  const { data: rooms } = useQuery({
    queryKey: ["org-rooms", group?.branchId],
    queryFn: () => roomsApi.list({ branchId: group!.branchId, isActive: true }),
    enabled: !!group?.branchId,
  });

  const { data: students } = useQuery({
    queryKey: ["crm-students-all"],
    queryFn: () => studentsApi.list({ isActive: true, pageSize: 200 }),
  });

  const invalidate = () => {
    qc.invalidateQueries({ queryKey: ["crm-group", groupId] });
    qc.invalidateQueries({ queryKey: ["crm-groups"] });
  };

  const assignTeacherMutation = useMutation({
    mutationFn: (teacherId: string | null) => groupsApi.assignTeacher(groupId!, teacherId),
    onSuccess: () => { invalidate(); toast({ title: "O'qituvchi tayinlandi" }); },
  });

  const assignRoomMutation = useMutation({
    mutationFn: (roomId: string | null) => groupsApi.assignRoom(groupId!, roomId),
    onSuccess: () => { invalidate(); toast({ title: "Xona tayinlandi" }); },
    onError: (e: unknown) => {
      const msg = (e as any)?.response?.data?.detail;
      toast({ title: "Xatolik", description: msg ?? "Xonani tayinlab bo'lmadi", variant: "destructive" });
    },
  });

  const addStudentMutation = useMutation({
    mutationFn: (studentId: string) => groupsApi.addStudent(groupId!, studentId),
    onSuccess: () => { invalidate(); setAddStudentId(""); toast({ title: "Talaba qo'shildi" }); },
    onError: (e: unknown) => {
      const msg = (e as any)?.response?.data?.detail;
      toast({ title: "Xatolik", description: msg ?? "Talaba qo'shib bo'lmadi", variant: "destructive" });
    },
  });

  const removeStudentMutation = useMutation({
    mutationFn: (studentId: string) => groupsApi.removeStudent(groupId!, studentId),
    onSuccess: invalidate,
  });

  const saveScheduleMutation = useMutation({
    mutationFn: (slots: GroupScheduleSlotDto[]) =>
      groupsApi.setSchedule(groupId!, slots.map((s) => ({
        ...s,
        startTime: withSeconds(s.startTime),
        endTime: withSeconds(s.endTime),
      }))),
    onSuccess: () => { invalidate(); setScheduleDraft(null); toast({ title: "Dars jadvali saqlandi" }); },
    onError: () => {
      toast({ title: "Xatolik", description: "Dars jadvalini saqlab bo'lmadi", variant: "destructive" });
    },
  });

  const recordPaymentMutation = useMutation({
    mutationFn: (data: { studentId: string; amount: number; paidAt: string; forMonth: string; method: import("@/types").PaymentMethod }) =>
      financeApi.recordPayment({ groupId: groupId!, ...data }),
    onSuccess: () => { invalidate(); setPaymentStudent(null); toast({ title: "To'lov qabul qilindi" }); },
    onError: () => {
      toast({ title: "Xatolik", description: "To'lovni saqlab bo'lmadi", variant: "destructive" });
    },
  });

  const statusMutation = useMutation({
    mutationFn: ({ studentId, status }: { studentId: string; status: GroupMembershipStatus }) =>
      groupsApi.setMembershipStatus(groupId!, studentId, status),
    onSuccess: invalidate,
  });

  const setDiscountMutation = useMutation({
    mutationFn: (data: { studentId: string; price: number; startDate: string; endDate: string }) =>
      groupsApi.setDiscount(groupId!, data.studentId, data),
    onSuccess: () => { invalidate(); setDiscountStudent(null); toast({ title: "Chegirma saqlandi" }); },
  });

  const removeDiscountMutation = useMutation({
    mutationFn: (studentId: string) => groupsApi.removeDiscount(groupId!, studentId),
    onSuccess: () => { invalidate(); setDiscountStudent(null); toast({ title: "Chegirma olib tashlandi" }); },
  });

  if (isLoading || !group) return <PageLoader />;

  const schedule = scheduleDraft ?? group.schedule ?? [];
  const availableStudents = (students?.items ?? []).filter(
    (s) => !group.students.some((gs) => gs.studentId === s.id)
  );

  const applyPreset = (days: DayOfWeek[]) => {
    const endTime = addHours(presetTime, 2);
    const kept = schedule.filter((s) => !days.includes(s.dayOfWeek));
    const added = days.map((d) => ({ dayOfWeek: d, startTime: presetTime, endTime }));
    setScheduleDraft([...kept, ...added]);
  };

  return (
    <div className="max-w-6xl mx-auto space-y-4">
      <Button variant="ghost" size="sm" onClick={() => navigate("/crm/groups")} className="-ml-2">
        <ArrowLeft className="h-4 w-4 mr-1.5" />
        Guruhlar
      </Button>

      <div className="grid grid-cols-1 lg:grid-cols-[340px_1fr] gap-4 items-start">
        {/* LEFT: persistent info + roster */}
        <div className="space-y-4">
          <Card>
            <CardContent className="p-5 space-y-4">
              <div>
                <div className="flex items-center gap-2 flex-wrap">
                  <h1 className="text-lg font-bold leading-tight">{group.name}</h1>
                  <Badge variant={group.isActive ? "success" : "secondary"} className="text-xs">
                    {group.isActive ? "Faol" : "Nofaol"}
                  </Badge>
                </div>
                {group.description && (
                  <p className="text-xs text-muted-foreground mt-1">{group.description}</p>
                )}
              </div>

              <Separator />

              <div className="flex items-center justify-between text-sm">
                <span className="text-muted-foreground">Narx</span>
                <span className="font-semibold">{group.price.toLocaleString()} so'm</span>
              </div>

              <InfoRow label="O'qituvchi">
                <Select
                  value={group.teacherId ?? "none"}
                  onValueChange={(v) => assignTeacherMutation.mutate(v === "none" ? null : v)}
                >
                  <SelectTrigger className="h-9">
                    <SelectValue placeholder="O'qituvchi tanlang" />
                  </SelectTrigger>
                  <SelectContent>
                    <SelectItem value="none">Tayinlanmagan</SelectItem>
                    {teachers?.map((t) => (
                      <SelectItem key={t.id} value={t.id}>
                        {t.fullName || t.phone || t.id}
                      </SelectItem>
                    ))}
                  </SelectContent>
                </Select>
              </InfoRow>

              <InfoRow label="Xona">
                <Select
                  value={group.roomId ?? "none"}
                  onValueChange={(v) => assignRoomMutation.mutate(v === "none" ? null : v)}
                >
                  <SelectTrigger className="h-9">
                    <SelectValue placeholder="Xona tanlang" />
                  </SelectTrigger>
                  <SelectContent>
                    <SelectItem value="none">Tayinlanmagan</SelectItem>
                    {rooms?.map((r) => (
                      <SelectItem key={r.id} value={r.id}>
                        {r.name} ({r.capacity} kishi)
                      </SelectItem>
                    ))}
                  </SelectContent>
                </Select>
                {group.roomId && group.roomCapacity != null && (
                  <p className="text-xs text-muted-foreground">
                    {group.students.length} / {group.roomCapacity} o'rin band
                  </p>
                )}
              </InfoRow>
            </CardContent>
          </Card>

          <Card>
            <CardContent className="p-5 space-y-3">
              <div className="flex items-center justify-between">
                <span className="text-sm font-semibold flex items-center gap-2">
                  <Users className="h-4 w-4 text-muted-foreground" />
                  Talabalar
                </span>
                <Badge variant="outline" className="text-xs">{group.students.length} ta</Badge>
              </div>

              <div className="flex gap-2">
                <Select value={addStudentId} onValueChange={setAddStudentId}>
                  <SelectTrigger className="h-9">
                    <SelectValue placeholder="Qo'shish..." />
                  </SelectTrigger>
                  <SelectContent>
                    {availableStudents.map((s) => (
                      <SelectItem key={s.id} value={s.id}>
                        {s.fullName} — {s.phone}
                      </SelectItem>
                    ))}
                  </SelectContent>
                </Select>
                <Button
                  variant="outline"
                  size="sm"
                  className="h-9"
                  disabled={!addStudentId || addStudentMutation.isPending}
                  onClick={() => addStudentMutation.mutate(addStudentId)}
                >
                  <Plus className="h-4 w-4" />
                </Button>
              </div>

              <div className="space-y-0.5 max-h-[420px] overflow-y-auto -mx-1.5 px-1.5">
                {group.students.length === 0 && (
                  <p className="text-sm text-muted-foreground text-center py-8">Talabalar yo'q</p>
                )}
                {group.students.map((s, i) => (
                  <div
                    key={s.studentId}
                    className="flex items-center gap-2 rounded-md px-1.5 py-2 hover:bg-muted/50 transition-colors group"
                  >
                    <span className="text-xs text-muted-foreground w-4 text-right flex-shrink-0">{i + 1}.</span>
                    <span
                      className={`h-1.5 w-1.5 rounded-full flex-shrink-0 ${s.balance < 0 ? "bg-destructive" : "bg-emerald-500"}`}
                      title={s.isDebtor ? "Qarzdor" : MEMBERSHIP_STATUS_LABELS[s.status]}
                    />
                    <button
                      className="text-sm font-medium hover:underline truncate flex-1 text-left min-w-0"
                      onClick={() => navigate(`/crm/students/${s.studentId}`)}
                      title={s.phone}
                    >
                      {s.fullName}
                    </button>
                    {s.status !== "Active" && (
                      <Badge variant={membershipBadgeVariant(s)} className="text-[10px] px-1.5 py-0 flex-shrink-0">
                        {MEMBERSHIP_STATUS_LABELS[s.status]}
                      </Badge>
                    )}
                    <span className={`text-xs font-medium flex-shrink-0 ${s.balance < 0 ? "text-destructive" : "text-emerald-500"}`}>
                      {s.balance.toLocaleString()}
                    </span>
                    <DropdownMenu>
                      <DropdownMenuTrigger asChild>
                        <Button variant="ghost" size="icon" className="h-7 w-7 flex-shrink-0">
                          <MoreVertical className="h-4 w-4" />
                        </Button>
                      </DropdownMenuTrigger>
                      <DropdownMenuContent align="end">
                        <DropdownMenuItem onClick={() => setPaymentStudent(s)}>
                          <Wallet className="h-4 w-4 mr-2" />
                          To'lov qabul qilish
                        </DropdownMenuItem>
                        <DropdownMenuItem
                          disabled={s.status === "Trial" || s.status === "Left"}
                          onClick={() => statusMutation.mutate({
                            studentId: s.studentId,
                            status: s.status === "Frozen" ? "Active" : "Frozen",
                          })}
                        >
                          <Snowflake className="h-4 w-4 mr-2" />
                          {s.status === "Frozen" ? "Faollashtirish" : "Muzlatish"}
                        </DropdownMenuItem>
                        <DropdownMenuItem onClick={() => setDiscountStudent(s)}>
                          <Percent className="h-4 w-4 mr-2" />
                          Chegirma
                        </DropdownMenuItem>
                        <DropdownMenuSeparator />
                        <DropdownMenuItem
                          className="text-destructive focus:text-destructive"
                          onClick={() => removeStudentMutation.mutate(s.studentId)}
                        >
                          <Trash2 className="h-4 w-4 mr-2" />
                          Guruhdan chiqarish
                        </DropdownMenuItem>
                      </DropdownMenuContent>
                    </DropdownMenu>
                  </div>
                ))}
              </div>
            </CardContent>
          </Card>
        </div>

        {/* RIGHT: tabbed content */}
        <Card>
          <CardContent className="p-5">
            <Tabs defaultValue="attendance">
              <TabsList>
                <TabsTrigger value="attendance">Davomat</TabsTrigger>
                <TabsTrigger value="schedule">Dars jadvali</TabsTrigger>
              </TabsList>

              <TabsContent value="attendance" className="mt-4">
                <AttendanceDialog groupId={group.id} groupName={group.name} inline />
              </TabsContent>

              <TabsContent value="schedule" className="mt-4 space-y-4">
                <div className="flex items-center gap-2 flex-wrap">
                  <Input
                    type="time"
                    value={presetTime}
                    onChange={(e) => setPresetTime(e.target.value)}
                    className="w-28"
                    title="Vaqt"
                  />
                  <Button variant="outline" size="sm" onClick={() => applyPreset(ODD_DAYS)}>
                    Toq kunlar (Dush, Chor, Juma)
                  </Button>
                  <Button variant="outline" size="sm" onClick={() => applyPreset(EVEN_DAYS)}>
                    Juft kunlar (Sesh, Pay, Shan)
                  </Button>
                </div>

                <div className="space-y-1.5">
                  {schedule.map((slot, i) => (
                    <div key={i} className="flex items-center gap-2 rounded-lg border bg-muted/20 px-3 py-2">
                      <Select
                        value={slot.dayOfWeek}
                        onValueChange={(v) => {
                          const next = [...schedule];
                          next[i] = { ...slot, dayOfWeek: v as DayOfWeek };
                          setScheduleDraft(next);
                        }}
                      >
                        <SelectTrigger className="w-28 bg-background">
                          <SelectValue />
                        </SelectTrigger>
                        <SelectContent>
                          {DAYS.map((day) => (
                            <SelectItem key={day} value={day}>{DAY_LABELS[day]}</SelectItem>
                          ))}
                        </SelectContent>
                      </Select>
                      <Input
                        type="time"
                        value={slot.startTime.slice(0, 5)}
                        onChange={(e) => {
                          const next = [...schedule];
                          next[i] = { ...slot, startTime: e.target.value, endTime: addHours(e.target.value, 2) };
                          setScheduleDraft(next);
                        }}
                        className="w-28 bg-background"
                      />
                      <span className="text-muted-foreground text-sm">—</span>
                      <Input
                        type="time"
                        value={slot.endTime.slice(0, 5)}
                        onChange={(e) => {
                          const next = [...schedule];
                          next[i] = { ...slot, endTime: e.target.value };
                          setScheduleDraft(next);
                        }}
                        className="w-28 bg-background"
                      />
                      <button
                        onClick={() => setScheduleDraft(schedule.filter((_, idx) => idx !== i))}
                        className="ml-auto h-8 w-8 flex items-center justify-center text-muted-foreground hover:text-destructive rounded-md"
                      >
                        <Trash2 className="h-4 w-4" />
                      </button>
                    </div>
                  ))}
                  {schedule.length === 0 && (
                    <p className="text-sm text-muted-foreground text-center py-6">Dars jadvali belgilanmagan</p>
                  )}
                  <Button
                    variant="outline"
                    size="sm"
                    onClick={() => {
                      const usedDays = new Set(schedule.map((s) => s.dayOfWeek));
                      const nextDay = DAYS.find((d) => !usedDays.has(d)) ?? "Monday";
                      setScheduleDraft([...schedule, { dayOfWeek: nextDay, startTime: presetTime, endTime: addHours(presetTime, 2) }]);
                    }}
                  >
                    <Plus className="h-4 w-4 mr-1" />
                    Kun qo'shish
                  </Button>
                </div>
                {scheduleDraft && (
                  <div className="flex gap-2 justify-end pt-3 border-t">
                    <Button variant="outline" size="sm" onClick={() => setScheduleDraft(null)}>Bekor</Button>
                    <Button
                      size="sm"
                      disabled={saveScheduleMutation.isPending}
                      onClick={() => saveScheduleMutation.mutate(scheduleDraft)}
                    >
                      Jadvalni saqlash
                    </Button>
                  </div>
                )}
              </TabsContent>
            </Tabs>
          </CardContent>
        </Card>
      </div>

      {paymentStudent && (
        <RecordPaymentDialog
          student={paymentStudent}
          isPending={recordPaymentMutation.isPending}
          onSubmit={(data) => recordPaymentMutation.mutate({ studentId: paymentStudent.studentId, ...data })}
          onClose={() => setPaymentStudent(null)}
        />
      )}

      {discountStudent && (
        <DiscountDialog
          student={discountStudent}
          isPending={setDiscountMutation.isPending || removeDiscountMutation.isPending}
          onSave={(data) => setDiscountMutation.mutate({ studentId: discountStudent.studentId, ...data })}
          onRemove={() => removeDiscountMutation.mutate(discountStudent.studentId)}
          onClose={() => setDiscountStudent(null)}
        />
      )}
    </div>
  );
}
