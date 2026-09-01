using MediatR;

namespace OnlineTesting.Application.Crm.Finance.Queries.GetPaymentsSummary;

public record GetPaymentsSummaryQuery(
    Guid? GroupId = null,
    Guid? StudentId = null,
    Guid? BranchId = null,
    DateOnly? FromDate = null,
    DateOnly? ToDate = null,
    string? Search = null) : IRequest<PaymentsSummaryDto>;

public record PaymentsSummaryDto(decimal TotalAmount, int Count);
