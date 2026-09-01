using MediatR;
using Microsoft.EntityFrameworkCore;
using OnlineTesting.Application.Common.Interfaces;
using OnlineTesting.Application.Common.Models;

namespace OnlineTesting.Application.Crm.Leads.Queries.GetLeads;

public class GetLeadsHandler : IRequestHandler<GetLeadsQuery, PagedResult<LeadDto>>
{
    private readonly IApplicationDbContext _db;

    public GetLeadsHandler(IApplicationDbContext db) => _db = db;

    public async Task<PagedResult<LeadDto>> Handle(GetLeadsQuery request, CancellationToken ct)
    {
        var page = Math.Max(1, request.Page);
        var size = Math.Clamp(request.PageSize, 1, 100);

        var query = _db.Leads.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var term = request.Search.Trim().ToLower();
            query = query.Where(l => l.FullName.ToLower().Contains(term) || l.Phone.ToLower().Contains(term));
        }

        if (request.Source.HasValue)
            query = query.Where(l => l.Source == request.Source.Value);

        if (request.Stage.HasValue)
            query = query.Where(l => l.Stage == request.Stage.Value);

        if (request.BranchId.HasValue)
            query = query.Where(l => l.BranchId == request.BranchId.Value);

        if (request.AssignedManagerId.HasValue)
            query = query.Where(l => l.AssignedManagerId == request.AssignedManagerId.Value);

        var total = await query.CountAsync(ct);

        var items = await query
            .OrderByDescending(l => l.CreatedAt)
            .Skip((page - 1) * size)
            .Take(size)
            .Select(l => new LeadDto(
                l.Id, l.BranchId, l.FullName, l.Phone, l.Email, l.Source.ToString(), l.Stage.ToString(),
                l.AssignedManagerId, l.Notes, l.LostReason, l.CreatedAt))
            .ToListAsync(ct);

        return new PagedResult<LeadDto>(items, page, size, total);
    }
}
