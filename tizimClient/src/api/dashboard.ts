import { api } from "./axios";

export interface DashboardSummaryDto {
  activeLeads: number;
  activeStudents: number;
  activeGroups: number;
  debtors: number;
  inTrial: number;
  paidThisMonthCount: number;
  paidThisMonthAmount: number;
}

export const dashboardApi = {
  getSummary: (branchId?: string) =>
    api.get<DashboardSummaryDto>("/crm/dashboard", { params: { branchId } }).then((r) => r.data),
};
