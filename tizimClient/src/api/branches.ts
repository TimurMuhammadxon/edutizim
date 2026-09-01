import { api } from "./axios";
import { type BranchDto } from "@/types";

export const branchesApi = {
  list: (params?: { isActive?: boolean }) =>
    api.get<BranchDto[]>("/org/branches", { params }).then((r) => r.data),

  getById: (id: string) =>
    api.get<BranchDto>(`/org/branches/${id}`).then((r) => r.data),

  create: (data: { name: string; address?: string }) =>
    api.post<{ id: string }>("/org/branches", data).then((r) => r.data),

  update: (id: string, data: { name: string; address?: string }) =>
    api.put(`/org/branches/${id}`, data),

  toggleActive: (id: string, isActive: boolean) =>
    api.patch(`/org/branches/${id}/active`, { isActive }),
};
