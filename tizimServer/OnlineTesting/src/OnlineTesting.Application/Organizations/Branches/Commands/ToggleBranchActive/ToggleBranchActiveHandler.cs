using MediatR;
using Microsoft.EntityFrameworkCore;
using OnlineTesting.Application.Common.Exceptions;
using OnlineTesting.Application.Common.Interfaces;

namespace OnlineTesting.Application.Organizations.Branches.Commands.ToggleBranchActive;

public class ToggleBranchActiveHandler : IRequestHandler<ToggleBranchActiveCommand>
{
    private readonly IApplicationDbContext _db;

    public ToggleBranchActiveHandler(IApplicationDbContext db) => _db = db;

    public async Task Handle(ToggleBranchActiveCommand request, CancellationToken ct)
    {
        var branch = await _db.Branches.FirstOrDefaultAsync(b => b.Id == request.Id, ct)
            ?? throw new NotFoundException($"Branch '{request.Id}' not found.");

        if (request.IsActive) branch.Activate();
        else branch.Deactivate();

        await _db.SaveChangesAsync(ct);
    }
}
