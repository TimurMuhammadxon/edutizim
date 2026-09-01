using MediatR;
using Microsoft.EntityFrameworkCore;
using OnlineTesting.Application.Common.Exceptions;
using OnlineTesting.Application.Common.Interfaces;
using OnlineTesting.Application.Crm.Tasks.Queries.GetTasks;

namespace OnlineTesting.Application.Crm.Tasks.Queries.GetTaskById;

public class GetTaskByIdHandler : IRequestHandler<GetTaskByIdQuery, CrmTaskDto>
{
    private readonly IApplicationDbContext _db;

    public GetTaskByIdHandler(IApplicationDbContext db) => _db = db;

    public async Task<CrmTaskDto> Handle(GetTaskByIdQuery request, CancellationToken ct)
    {
        var row = await (
            from t in _db.CrmTasks.AsNoTracking()
            join l in _db.Leads.AsNoTracking() on t.LeadId equals (Guid?)l.Id into leadJoin
            from l in leadJoin.DefaultIfEmpty()
            where t.Id == request.Id
            select new { Task = t, Lead = l }
        ).FirstOrDefaultAsync(ct)
            ?? throw new NotFoundException($"Task '{request.Id}' not found.");

        return new CrmTaskDto(
            row.Task.Id, row.Task.Title, row.Task.Description, row.Task.DueAt, row.Task.Status.ToString(),
            row.Task.AssignedToUserId, row.Task.LeadId, row.Lead?.FullName,
            row.Task.CreatedAt, row.Task.CompletedAt);
    }
}
