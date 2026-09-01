using MediatR;
using OnlineTesting.Application.Common.Models;
using OnlineTesting.Domain.Crm;

namespace OnlineTesting.Application.Crm.Tasks.Queries.GetTasks;

public record GetTasksQuery(
    CrmTaskStatus? Status,
    Guid? AssignedToUserId,
    int Page = 1,
    int PageSize = 50) : IRequest<PagedResult<CrmTaskDto>>;

public record CrmTaskDto(
    Guid Id,
    string Title,
    string? Description,
    DateTime DueAt,
    string Status,
    Guid AssignedToUserId,
    Guid? LeadId,
    string? LeadFullName,
    DateTime CreatedAt,
    DateTime? CompletedAt);
