using MediatR;

namespace OnlineTesting.Application.Crm.Dashboard.Queries.GetDashboardSummary;

public record GetDashboardSummaryQuery(Guid? BranchId) : IRequest<DashboardSummaryDto>;

public record DashboardSummaryDto(
    int ActiveLeads,
    int ActiveStudents,
    int ActiveGroups,
    int Debtors,
    int InTrial,
    int PaidThisMonthCount,
    decimal PaidThisMonthAmount);
