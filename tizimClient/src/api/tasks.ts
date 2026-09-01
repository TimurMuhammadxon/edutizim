import { api } from "./axios";
import { type CrmTaskDto, type CrmTaskStatus, type PagedResult } from "@/types";

export const tasksApi = {
  list: (params?: { status?: CrmTaskStatus; assignedToUserId?: string; page?: number; pageSize?: number }) =>
    api.get<PagedResult<CrmTaskDto>>("/crm/tasks", { params }).then((r) => r.data),

  getById: (id: string) =>
    api.get<CrmTaskDto>(`/crm/tasks/${id}`).then((r) => r.data),

  create: (data: { title: string; description?: string; dueAt: string; assignedToUserId: string; leadId?: string }) =>
    api.post<{ id: string }>("/crm/tasks", data).then((r) => r.data),

  complete: (id: string) => api.post(`/crm/tasks/${id}/complete`),

  cancel: (id: string) => api.post(`/crm/tasks/${id}/cancel`),

  reschedule: (id: string, dueAt: string) => api.patch(`/crm/tasks/${id}/reschedule`, { dueAt }),
};
