import { api } from "./axios";
import { type MemberDto, type Role } from "@/types";

export const membersApi = {
  list: (role?: Role) =>
    api.get<MemberDto[]>("/org/members", { params: { role } }).then((r) => r.data),

  createStaff: (data: { phone: string; password: string; firstName?: string; lastName?: string }) =>
    api.post<{ id: string }>("/org/members/staff", data).then((r) => r.data),

  createTeacher: (data: { phone: string; password: string; firstName?: string; lastName?: string }) =>
    api.post<{ id: string }>("/org/members/teachers", data).then((r) => r.data),

  deactivate: (id: string) => api.patch(`/org/members/${id}/deactivate`),
};
