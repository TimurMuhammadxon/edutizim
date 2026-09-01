using MediatR;

namespace OnlineTesting.Application.Crm.Tasks.Commands.RescheduleTask;

public record RescheduleTaskCommand(Guid Id, DateTime DueAt) : IRequest;
