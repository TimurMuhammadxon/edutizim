import { api } from "./axios";
import { type LeadDto, type ClientSource, type LeadStage, type PagedResult } from "@/types";

export const leadsApi = {
  list: (params?: {
    search?: string;
    source?: ClientSource;
    stage?: LeadStage;
    branchId?: string;
    assignedManagerId?: string;
    page?: number;
    pageSize?: number;
  }) => api.get<PagedResult<LeadDto>>("/crm/leads", { params }).then((r) => r.data),

  getById: (id: string) =>
    api.get<LeadDto>(`/crm/leads/${id}`).then((r) => r.data),

  create: (data: { branchId: string; fullName: string; phone: string; source: ClientSource; assignedManagerId?: string }) =>
    api.post<{ id: string }>("/crm/leads", data).then((r) => r.data),

  update: (id: string, data: { fullName: string; phone: string; email?: string; notes?: string }) =>
    api.put(`/crm/leads/${id}`, data),

  assignManager: (id: string, managerId: string | null) =>
    api.patch(`/crm/leads/${id}/manager`, { managerId }),

  changeStage: (id: string, stage: LeadStage, lostReason?: string) =>
    api.patch(`/crm/leads/${id}/stage`, { stage, lostReason }),

  convertToStudent: (id: string) =>
    api.post<{ studentId: string }>(`/crm/leads/${id}/convert`).then((r) => r.data),
};
