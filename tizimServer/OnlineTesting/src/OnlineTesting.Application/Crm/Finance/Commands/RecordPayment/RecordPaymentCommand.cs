using MediatR;
using OnlineTesting.Domain.Crm;

namespace OnlineTesting.Application.Crm.Finance.Commands.RecordPayment;

public record RecordPaymentCommand(Guid GroupId, Guid StudentId, decimal Amount, DateOnly PaidAt, DateOnly ForMonth, PaymentMethod Method, string? Note) : IRequest<Guid>;
