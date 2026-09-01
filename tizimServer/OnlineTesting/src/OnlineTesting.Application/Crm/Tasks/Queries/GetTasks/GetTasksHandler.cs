using MediatR;
using Microsoft.EntityFrameworkCore;
using OnlineTesting.Application.Common.Interfaces;
using OnlineTesting.Application.Common.Models;

namespace OnlineTesting.Application.Crm.Tasks.Queries.GetTasks;

public class GetTasksHandler : IRequestHandler<GetTasksQuery, PagedResult<CrmTaskDto>>
{
    private readonly IApplicationDbContext _db;

    public GetTasksHandler(IApplicationDbContext db) => _db = db;

    public async Task<PagedResult<CrmTaskDto>> Handle(GetTasksQuery request, CancellationToken ct)
    {
        var page = Math.Max(1, request.Page);
        var size = Math.Clamp(request.PageSize, 1, 200);

        var query =
            from t in _db.CrmTasks.AsNoTracking()
            join l in _db.Leads.AsNoTracking() on t.LeadId equals (Guid?)l.Id into leadJoin
            from l in leadJoin.DefaultIfEmpty()
            select new { Task = t, Lead = l };

        if (request.Status.HasValue)
            query = query.Where(x => x.Task.Status == request.Status.Value);

        if (request.AssignedToUserId.HasValue)
            query = query.Where(x => x.Task.AssignedToUserId == request.AssignedToUserId.Value);

        var total = await query.CountAsync(ct);

        var rows = await query
            .OrderBy(x => x.Task.DueAt)
            .Skip((page - 1) * size)
            .Take(size)
            .ToListAsync(ct);

        var items = rows.Select(x => new CrmTaskDto(
            x.Task.Id, x.Task.Title, x.Task.Description, x.Task.DueAt, x.Task.Status.ToString(),
            x.Task.AssignedToUserId, x.Task.LeadId, x.Lead?.FullName,
            x.Task.CreatedAt, x.Task.CompletedAt))
            .ToList();

        return new PagedResult<CrmTaskDto>(items, page, size, total);
    }
}
