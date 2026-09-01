import { useQuery } from "@tanstack/react-query";
import { useState } from "react";
import { groupsApi } from "@/api/groups";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { Dialog, DialogContent, DialogHeader, DialogTitle } from "@/components/ui/dialog";
import { PageLoader } from "@/components/shared/LoadingSpinner";
import { AttendanceDialog } from "@/components/shared/AttendanceDialog";
import { Users, Clock, CalendarCheck } from "lucide-react";
import type { GroupDto, DayOfWeek } from "@/types";

const DAY_LABELS: Record<DayOfWeek, string> = {
  Monday: "Dush", Tuesday: "Sesh", Wednesday: "Chor", Thursday: "Pay",
  Friday: "Jum", Saturday: "Shan", Sunday: "Yak",
};

export function MyGroupsPage() {
  const [selectedGroupId, setSelectedGroupId] = useState<string | null>(null);
  const [attendanceGroup, setAttendanceGroup] = useState<GroupDto | null>(null);

  const { data, isLoading } = useQuery({
    queryKey: ["my-groups"],
    queryFn: () => groupsApi.list({ pageSize: 100 }),
  });

  const { data: selectedGroup, isLoading: loadingDetails } = useQuery({
    queryKey: ["my-group", selectedGroupId],
    queryFn: () => groupsApi.getById(selectedGroupId!),
    enabled: !!selectedGroupId,
  });

  if (isLoading) return <PageLoader />;

  return (
    <div className="max-w-4xl mx-auto space-y-6">
      <div>
        <h1 className="text-2xl font-bold">Mening guruhlarim</h1>
        <p className="text-muted-foreground mt-1">Sizga tayinlangan guruhlar</p>
      </div>

      <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-4">
        {data?.items.map((group: GroupDto) => (
          <Card
            key={group.id}
            className="hover:shadow-md transition-shadow cursor-pointer"
            onClick={() => setSelectedGroupId(group.id)}
          >
            <CardHeader className="pb-2">
              <div className="flex items-start justify-between">
                <CardTitle className="text-base">{group.name}</CardTitle>
                <Badge variant={group.isActive ? "success" : "secondary"} className="text-xs">
                  {group.isActive ? "Faol" : "Nofaol"}
                </Badge>
              </div>
            </CardHeader>
            <CardContent className="space-y-3">
              <div className="flex items-center gap-2 text-sm text-muted-foreground">
                <Users className="h-4 w-4" />
                {group.studentCount} ta talaba
              </div>
              <Button
                variant="outline"
                size="sm"
                className="w-full"
                onClick={(e) => { e.stopPropagation(); setAttendanceGroup(group); }}
              >
                <CalendarCheck className="h-4 w-4 mr-1.5" />
                Davomat
              </Button>
            </CardContent>
          </Card>
        ))}

        {data?.items.length === 0 && (
          <Card className="col-span-full">
            <CardContent className="flex flex-col items-center justify-center py-12 text-center">
              <Users className="h-12 w-12 text-muted-foreground mb-3" />
              <p className="text-muted-foreground">Sizga hali guruh tayinlanmagan</p>
            </CardContent>
          </Card>
        )}
      </div>

      <Dialog open={!!selectedGroupId} onOpenChange={(o) => !o && setSelectedGroupId(null)}>
        <DialogContent className="max-w-lg">
          <DialogHeader>
            <DialogTitle>{selectedGroup?.name ?? "Guruh"}</DialogTitle>
          </DialogHeader>

          {loadingDetails || !selectedGroup ? (
            <PageLoader />
          ) : (
            <div className="space-y-4">
              {selectedGroup.schedule.length > 0 && (
                <div className="space-y-1.5">
                  <p className="text-sm font-medium flex items-center gap-1.5">
                    <Clock className="h-4 w-4" /> Dars jadvali
                  </p>
                  <div className="text-sm text-muted-foreground space-y-0.5">
                    {selectedGroup.schedule.map((slot, i) => (
                      <div key={i}>
                        {DAY_LABELS[slot.dayOfWeek]}: {slot.startTime.slice(0, 5)}–{slot.endTime.slice(0, 5)}
                      </div>
                    ))}
                  </div>
                </div>
              )}

              <div className="space-y-1.5">
                <p className="text-sm font-medium">Talabalar ({selectedGroup.students.length})</p>
                <div className="border rounded-md divide-y max-h-64 overflow-y-auto">
                  {selectedGroup.students.length === 0 && (
                    <p className="text-sm text-muted-foreground text-center py-6">Talabalar yo'q</p>
                  )}
                  {selectedGroup.students.map((s) => (
                    <div key={s.studentId} className="px-3 py-2 text-sm">
                      <div className="font-medium">{s.fullName}</div>
                      <div className="text-xs text-muted-foreground">{s.phone}</div>
                    </div>
                  ))}
                </div>
              </div>
            </div>
          )}
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
