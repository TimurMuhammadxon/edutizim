using MediatR;

namespace OnlineTesting.Application.Crm.Tasks.Commands.CreateTask;

public record CreateTaskCommand(
    string Title,
    string? Description,
    DateTime DueAt,
    Guid AssignedToUserId,
    Guid? LeadId) : IRequest<Guid>;
