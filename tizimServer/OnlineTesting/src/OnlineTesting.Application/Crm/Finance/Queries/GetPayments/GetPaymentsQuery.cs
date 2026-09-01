using MediatR;
using OnlineTesting.Application.Common.Models;
using OnlineTesting.Domain.Crm;

namespace OnlineTesting.Application.Crm.Finance.Queries.GetPayments;

public record GetPaymentsQuery(
    Guid? GroupId = null,
    Guid? StudentId = null,
    Guid? BranchId = null,
    DateOnly? FromDate = null,
    DateOnly? ToDate = null,
    string? Search = null,
    int Page = 1,
    int PageSize = 50) : IRequest<PagedResult<PaymentDto>>;

public record PaymentDto(
    Guid Id,
    Guid GroupId,
    string GroupName,
    Guid StudentId,
    string StudentFullName,
    decimal Amount,
    DateOnly PaidAt,
    DateOnly ForMonth,
    PaymentMethod Method,
    string? Note,
    DateTime CreatedAt);
