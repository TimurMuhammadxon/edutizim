import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import { useState } from "react";
import { attendanceApi } from "@/api/attendance";
import { Button } from "@/components/ui/button";
import { Dialog, DialogContent, DialogHeader, DialogTitle, DialogFooter } from "@/components/ui/dialog";
import { PageLoader } from "@/components/shared/LoadingSpinner";
import { ChevronLeft, ChevronRight, Check, X, CheckCheck } from "lucide-react";
import type { AttendanceStatus } from "@/types";

interface Props {
  groupId: string;
  groupName: string;
  onClose?: () => void;
  /** Render the grid directly (no Dialog wrapper) — for embedding as a page tab. */
  inline?: boolean;
}

const MONTH_NAMES = [
  "Yanvar", "Fevral", "Mart", "Aprel", "May", "Iyun",
  "Iyul", "Avgust", "Sentyabr", "Oktyabr", "Noyabr", "Dekabr",
];

function nextStatus(current: AttendanceStatus | undefined): AttendanceStatus | null {
  if (current === undefined) return "Present";
  if (current === "Present") return "Absent";
  return null;
}

function nextBulkStatus(column: (AttendanceStatus | undefined)[]): AttendanceStatus | null {
  const first = column[0];
  const allSame = column.every((m) => m === first);
  return allSame ? nextStatus(first) : "Present";
}

export function AttendanceDialog({ groupId, groupName, onClose, inline }: Props) {
  const qc = useQueryClient();
  const now = new Date();
  const [year, setYear] = useState(now.getFullYear());
  const [month, setMonth] = useState(now.getMonth() + 1);

  const { data, isLoading } = useQuery({
    queryKey: ["group-attendance", groupId, year, month],
    queryFn: () => attendanceApi.getForGroup(groupId, year, month),
  });

  const setMutation = useMutation({
    mutationFn: (vars: { studentId: string; lessonDate: string; status: AttendanceStatus | null }) =>
      attendanceApi.set(groupId, vars),
    onSuccess: () => qc.invalidateQueries({ queryKey: ["group-attendance", groupId, year, month] }),
  });

  const setForDateMutation = useMutation({
    mutationFn: (vars: { lessonDate: string; status: AttendanceStatus | null }) =>
      attendanceApi.setForDate(groupId, vars),
    onSuccess: () => qc.invalidateQueries({ queryKey: ["group-attendance", groupId, year, month] }),
  });

  const shiftMonth = (delta: number) => {
    let m = month + delta;
    let y = year;
    if (m < 1) { m = 12; y -= 1; }
    if (m > 12) { m = 1; y += 1; }
    setYear(y);
    setMonth(m);
  };

  const today = now.toISOString().slice(0, 10);

  const body = (
    <>
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
      ) : data.lessonDates.length === 0 ? (
        <p className="text-center text-muted-foreground py-10">
          Guruh uchun dars jadvali belgilanmagan
        </p>
      ) : (
        <div className="overflow-x-auto">
          <table className="border-collapse text-sm w-full">
            <thead>
              <tr>
                <th className="sticky left-0 bg-background text-left px-2 py-1.5 border-b font-medium min-w-[160px]">
                  Talaba
                </th>
                {data.lessonDates.map((d) => {
                  const isFuture = d > today;
                  const disabled = isFuture || data.students.length === 0 || setForDateMutation.isPending;
                  const target = nextBulkStatus(data.students.map((s) => s.marks[d]));
                  return (
                    <th key={d} className="px-1 py-1.5 border-b font-medium text-center w-9">
                      <div className="flex flex-col items-center gap-1">
                        <span>{d.slice(8, 10)}</span>
                        <button
                          disabled={disabled}
                          title="Barchasini belgilash"
                          onClick={() => setForDateMutation.mutate({ lessonDate: d, status: target })}
                          className={`h-5 w-5 rounded flex items-center justify-center ${
                            disabled ? "opacity-30 cursor-not-allowed" : "hover:bg-muted cursor-pointer text-muted-foreground"
                          }`}
                        >
                          <CheckCheck className="h-3.5 w-3.5" />
                        </button>
                      </div>
                    </th>
                  );
                })}
              </tr>
            </thead>
            <tbody>
              {data.students.map((s) => (
                <tr key={s.studentId}>
                  <td className="sticky left-0 bg-background px-2 py-1 border-b whitespace-nowrap">
                    {s.fullName}
                  </td>
                  {data.lessonDates.map((d) => {
                    const status = s.marks[d];
                    const isFuture = d > today;
                    return (
                      <td key={d} className="px-1 py-1 border-b text-center">
                        <button
                          disabled={isFuture || setMutation.isPending}
                          onClick={() => setMutation.mutate({ studentId: s.studentId, lessonDate: d, status: nextStatus(status) })}
                          className={`h-6 w-6 rounded flex items-center justify-center mx-auto ${
                            isFuture ? "opacity-30 cursor-not-allowed" : "hover:bg-muted cursor-pointer"
                          } ${
                            status === "Present"
                              ? "bg-emerald-500/15"
                              : status === "Absent"
                                ? "bg-red-500/15"
                                : "border border-dashed border-muted-foreground/30"
                          }`}
                        >
                          {status === "Present" && <Check className="h-3.5 w-3.5 text-emerald-500" />}
                          {status === "Absent" && <X className="h-3.5 w-3.5 text-red-500" />}
                        </button>
                      </td>
                    );
                  })}
                </tr>
              ))}
              {data.students.length === 0 && (
                <tr>
                  <td colSpan={data.lessonDates.length + 1} className="text-center text-muted-foreground py-6">
                    Talabalar yo'q
                  </td>
                </tr>
              )}
            </tbody>
          </table>
        </div>
      )}
    </>
  );

  if (inline) {
    return <div className="space-y-4">{body}</div>;
  }

  return (
    <Dialog open onOpenChange={(o) => !o && onClose?.()}>
      <DialogContent className="max-w-4xl">
        <DialogHeader>
          <DialogTitle>{groupName} — Davomat</DialogTitle>
        </DialogHeader>

        {body}

        <DialogFooter>
          <Button variant="outline" onClick={onClose}>Yopish</Button>
        </DialogFooter>
      </DialogContent>
    </Dialog>
  );
}
