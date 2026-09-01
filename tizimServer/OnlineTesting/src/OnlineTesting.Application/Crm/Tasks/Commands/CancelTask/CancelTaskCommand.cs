using MediatR;

namespace OnlineTesting.Application.Crm.Tasks.Commands.CancelTask;

public record CancelTaskCommand(Guid Id) : IRequest;
