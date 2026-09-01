using MediatR;
using OnlineTesting.Application.Crm.Tasks.Queries.GetTasks;

namespace OnlineTesting.Application.Crm.Tasks.Queries.GetTaskById;

public record GetTaskByIdQuery(Guid Id) : IRequest<CrmTaskDto>;
