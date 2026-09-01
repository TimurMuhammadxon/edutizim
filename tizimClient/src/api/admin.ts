import { api } from "./axios";
import { type PagedResult } from "@/types";

// --- Users (Owner) ---
export interface UserAdminDto {
  id: string;
  email?: string;
  phone?: string;
  firstName?: string;
  lastName?: string;
  role: string;
  isActive: boolean;
  createdAt: string;
}

export const adminUsersApi = {
  list: (params?: { search?: string; page?: number; pageSize?: number }) =>
    api.get<PagedResult<UserAdminDto>>("/admin/users", { params }).then((r) => r.data),
};
