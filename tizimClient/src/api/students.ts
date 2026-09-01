import { api } from "./axios";
import { type StudentDto, type StudentDetailsDto, type StudentAttendanceDto, type PagedResult } from "@/types";

export const studentsApi = {
  list: (params?: {
    search?: string;
    branchId?: string;
    groupId?: string;
    isActive?: boolean;
    studentStatus?: string;
    financialStatus?: string;
    page?: number;
    pageSize?: number;
  }) =>
    api.get<PagedResult<StudentDto>>("/crm/students", { params }).then((r) => r.data),

  getById: (id: string) =>
    api.get<StudentDetailsDto>(`/crm/students/${id}`).then((r) => r.data),

  getAttendance: (id: string, year: number, month: number) =>
    api.get<StudentAttendanceDto>(`/crm/students/${id}/attendance`, { params: { year, month } }).then((r) => r.data),

  create: (data: { branchId: string; fullName: string; phone: string; email?: string }) =>
    api.post<{ id: string }>("/crm/students", data).then((r) => r.data),

  update: (id: string, data: { fullName: string; phone: string; email?: string; notes?: string }) =>
    api.put(`/crm/students/${id}`, data),

  toggleActive: (id: string, isActive: boolean) =>
    api.patch(`/crm/students/${id}/active`, { isActive }),

  createLogin: (id: string, password: string) =>
    api.post<{ userId: string }>(`/crm/students/${id}/login`, { password }).then((r) => r.data),
};
