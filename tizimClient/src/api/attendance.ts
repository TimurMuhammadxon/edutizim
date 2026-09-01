import { api } from "./axios";
import { type GroupAttendanceDto, type AttendanceStatus } from "@/types";

export const attendanceApi = {
  getForGroup: (groupId: string, year: number, month: number) =>
    api.get<GroupAttendanceDto>(`/crm/groups/${groupId}/attendance`, { params: { year, month } }).then((r) => r.data),

  set: (groupId: string, data: { studentId: string; lessonDate: string; status: AttendanceStatus | null }) =>
    api.put(`/crm/groups/${groupId}/attendance`, data),

  setForDate: (groupId: string, data: { lessonDate: string; status: AttendanceStatus | null }) =>
    api.put(`/crm/groups/${groupId}/attendance/date`, data),
};
