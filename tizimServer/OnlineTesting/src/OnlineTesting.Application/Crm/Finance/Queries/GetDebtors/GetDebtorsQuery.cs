using MediatR;
using OnlineTesting.Application.Common.Models;

namespace OnlineTesting.Application.Crm.Finance.Queries.GetDebtors;

public record GetDebtorsQuery(
    Guid? BranchId,
    Guid? GroupId = null,
    string? Search = null,
    int Page = 1,
    int PageSize = 50) : IRequest<PagedResult<DebtorDto>>;

public record DebtorDto(
    Guid StudentId,
    string StudentFullName,
    string StudentPhone,
    Guid GroupId,
    string GroupName,
    decimal EffectivePrice,
    decimal Balance,
    DateOnly NextPaymentDueDate,
    int DaysOverdue);
