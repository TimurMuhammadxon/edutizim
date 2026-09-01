using MediatR;
using OnlineTesting.Application.Common.Models;

namespace OnlineTesting.Application.Crm.Finance.Queries.GetPeriodDebts;

public record GetPeriodDebtsQuery(
    DateOnly FromDate,
    DateOnly ToDate,
    Guid? BranchId = null,
    Guid? GroupId = null,
    string? Search = null,
    int Page = 1,
    int PageSize = 50) : IRequest<PagedResult<PeriodDebtDto>>;

public record PeriodDebtDto(
    Guid StudentId,
    string StudentFullName,
    string StudentPhone,
    Guid GroupId,
    string GroupName,
    decimal AmountOwedInPeriod,
    List<PeriodDebtMonthDto> Months);

public record PeriodDebtMonthDto(DateOnly Month, decimal Expected, decimal Paid, decimal Shortfall);
