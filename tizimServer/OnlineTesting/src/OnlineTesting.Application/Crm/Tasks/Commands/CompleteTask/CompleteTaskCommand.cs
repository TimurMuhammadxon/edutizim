using MediatR;

namespace OnlineTesting.Application.Crm.Tasks.Commands.CompleteTask;

public record CompleteTaskCommand(Guid Id) : IRequest;
