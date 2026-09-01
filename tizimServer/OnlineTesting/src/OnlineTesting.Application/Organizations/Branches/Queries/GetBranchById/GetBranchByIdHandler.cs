using MediatR;
using Microsoft.EntityFrameworkCore;
using OnlineTesting.Application.Common.Exceptions;
using OnlineTesting.Application.Common.Interfaces;
using OnlineTesting.Application.Organizations.Branches.Queries.GetBranches;

namespace OnlineTesting.Application.Organizations.Branches.Queries.GetBranchById;

public class GetBranchByIdHandler : IRequestHandler<GetBranchByIdQuery, BranchDto>
{
    private readonly IApplicationDbContext _db;

    public GetBranchByIdHandler(IApplicationDbContext db) => _db = db;

    public async Task<BranchDto> Handle(GetBranchByIdQuery request, CancellationToken ct)
    {
        var branch = await _db.Branches.AsNoTracking()
            .FirstOrDefaultAsync(b => b.Id == request.Id, ct)
            ?? throw new NotFoundException($"Branch '{request.Id}' not found.");

        return new BranchDto(branch.Id, branch.Name, branch.Address, branch.IsActive, branch.CreatedAt);
    }
}
