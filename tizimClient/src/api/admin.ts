import { api } from "./axios";
import {
  type SubscriptionPlanDto,
  type PagedResult,
} from "@/types";

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
  subscriptionExpiresAt?: string;
}

export const adminUsersApi = {
  list: (params?: { search?: string; page?: number; pageSize?: number }) =>
    api.get<PagedResult<UserAdminDto>>("/admin/users", { params }).then((r) => r.data),

  grantSubscription: (userId: string, planId: string) =>
    api.post(`/admin/users/${userId}/subscription`, { planId }),
};

// --- Payments (Owner) ---
export interface PaymentOrderAdminDto {
  id: string;
  userEmail: string;
  planLabel: string;
  amountTiyin: number;
  status: string;
  createdAt: string;
}

export const adminPaymentsApi = {
  list: (params?: { page?: number; pageSize?: number }) =>
    api.get<PagedResult<PaymentOrderAdminDto>>("/admin/payments", { params }).then((r) => r.data),
};

// --- Subscription plans ---
export const adminPlansApi = {
  list: () =>
    api.get<SubscriptionPlanDto[]>("/admin/subscription-plans").then((r) => r.data),

  setPlanPrice: (id: string, price: number) =>
    api.patch(`/admin/subscription-plans/${id}/price`, { price }),

  togglePlan: (id: string, isActive: boolean) =>
    api.patch(`/admin/subscription-plans/${id}/toggle`, { isActive }),

  grantSubscription: (userId: string, planId: string) =>
    api.post(`/admin/users/${userId}/subscription`, { planId }),
};
