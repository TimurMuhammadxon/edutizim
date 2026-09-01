using MediatR;
using Microsoft.EntityFrameworkCore;
using OnlineTesting.Application.Common.Interfaces;

namespace OnlineTesting.Application.Organizations.Branches.Queries.GetBranches;

public class GetBranchesHandler : IRequestHandler<GetBranchesQuery, List<BranchDto>>
{
    private readonly IApplicationDbContext _db;

    public GetBranchesHandler(IApplicationDbContext db) => _db = db;

    public async Task<List<BranchDto>> Handle(GetBranchesQuery request, CancellationToken ct)
    {
        var query = _db.Branches.AsNoTracking().AsQueryable();

        if (request.IsActive.HasValue)
            query = query.Where(b => b.IsActive == request.IsActive.Value);

        return await query
            .OrderBy(b => b.CreatedAt)
            .Select(b => new BranchDto(b.Id, b.Name, b.Address, b.IsActive, b.CreatedAt))
            .ToListAsync(ct);
    }
}
