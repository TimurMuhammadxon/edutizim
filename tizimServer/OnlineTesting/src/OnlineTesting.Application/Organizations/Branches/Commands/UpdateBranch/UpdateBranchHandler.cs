using MediatR;
using Microsoft.EntityFrameworkCore;
using OnlineTesting.Application.Common.Exceptions;
using OnlineTesting.Application.Common.Interfaces;

namespace OnlineTesting.Application.Organizations.Branches.Commands.UpdateBranch;

public class UpdateBranchHandler : IRequestHandler<UpdateBranchCommand>
{
    private readonly IApplicationDbContext _db;

    public UpdateBranchHandler(IApplicationDbContext db) => _db = db;

    public async Task Handle(UpdateBranchCommand request, CancellationToken ct)
    {
        var branch = await _db.Branches.FirstOrDefaultAsync(b => b.Id == request.Id, ct)
            ?? throw new NotFoundException($"Branch '{request.Id}' not found.");

        branch.Update(request.Name, request.Address);
        await _db.SaveChangesAsync(ct);
    }
}
