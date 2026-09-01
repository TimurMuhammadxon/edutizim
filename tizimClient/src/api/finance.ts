import { api } from "./axios";
import {
  type PaymentDto, type DebtorDto, type PaymentMethod, type PagedResult,
  type PaymentsSummaryDto, type PeriodDebtDto,
} from "@/types";

export interface FinanceFilterParams {
  groupId?: string;
  studentId?: string;
  branchId?: string;
  fromDate?: string;
  toDate?: string;
  search?: string;
}

export const financeApi = {
  getPayments: (params?: FinanceFilterParams & { page?: number; pageSize?: number }) =>
    api.get<PagedResult<PaymentDto>>("/crm/finance/payments", { params }).then((r) => r.data),

  getPaymentsSummary: (params?: FinanceFilterParams) =>
    api.get<PaymentsSummaryDto>("/crm/finance/payments/summary", { params }).then((r) => r.data),

  recordPayment: (data: { groupId: string; studentId: string; amount: number; paidAt: string; forMonth: string; method: PaymentMethod; note?: string }) =>
    api.post<{ id: string }>("/crm/finance/payments", data).then((r) => r.data),

  deletePayment: (id: string) =>
    api.delete(`/crm/finance/payments/${id}`),

  getDebtors: (params?: { branchId?: string; groupId?: string; search?: string }) =>
    api.get<DebtorDto[]>("/crm/finance/debtors", { params }).then((r) => r.data),

  getPeriodDebts: (params: { fromDate: string; toDate: string; branchId?: string; groupId?: string; search?: string }) =>
    api.get<PeriodDebtDto[]>("/crm/finance/period-debts", { params }).then((r) => r.data),
};
