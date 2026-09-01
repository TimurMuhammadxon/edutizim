import { api } from "./axios";
import { type GroupDto, type GroupDetailsDto, type GroupScheduleSlotDto, type GroupMembershipStatus, type PagedResult } from "@/types";

export const groupsApi = {
  list: (params?: { branchId?: string; teacherId?: string; isActive?: boolean; page?: number; pageSize?: number }) =>
    api.get<PagedResult<GroupDto>>("/crm/groups", { params }).then((r) => r.data),

  getById: (id: string) =>
    api.get<GroupDetailsDto>(`/crm/groups/${id}`).then((r) => r.data),

  create: (data: { branchId: string; name: string; price: number; description?: string }) =>
    api.post<{ id: string }>("/crm/groups", data).then((r) => r.data),

  update: (id: string, data: { name: string; price: number; description?: string }) =>
    api.put(`/crm/groups/${id}`, data),

  toggleActive: (id: string, isActive: boolean) =>
    api.patch(`/crm/groups/${id}/active`, { isActive }),

  assignTeacher: (id: string, teacherId: string | null) =>
    api.patch(`/crm/groups/${id}/teacher`, { teacherId }),

  assignRoom: (id: string, roomId: string | null) =>
    api.patch(`/crm/groups/${id}/room`, { roomId }),

  addStudent: (id: string, studentId: string) =>
    api.post(`/crm/groups/${id}/students`, { studentId }),

  removeStudent: (id: string, studentId: string) =>
    api.delete(`/crm/groups/${id}/students/${studentId}`),

  setSchedule: (id: string, slots: GroupScheduleSlotDto[]) =>
    api.put(`/crm/groups/${id}/schedule`, slots),

  setMembershipStatus: (id: string, studentId: string, status: GroupMembershipStatus) =>
    api.patch(`/crm/groups/${id}/students/${studentId}/status`, { status }),

  setDiscount: (id: string, studentId: string, data: { price: number; startDate: string; endDate: string }) =>
    api.put(`/crm/groups/${id}/students/${studentId}/discount`, data),

  removeDiscount: (id: string, studentId: string) =>
    api.delete(`/crm/groups/${id}/students/${studentId}/discount`),
};
