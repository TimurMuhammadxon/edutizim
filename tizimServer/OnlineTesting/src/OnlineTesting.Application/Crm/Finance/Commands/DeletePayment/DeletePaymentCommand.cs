using MediatR;

namespace OnlineTesting.Application.Crm.Finance.Commands.DeletePayment;

public record DeletePaymentCommand(Guid Id) : IRequest;
