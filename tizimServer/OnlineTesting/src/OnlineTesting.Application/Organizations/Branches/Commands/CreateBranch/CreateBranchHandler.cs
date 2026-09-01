using MediatR;
using OnlineTesting.Application.Common.Exceptions;
using OnlineTesting.Application.Common.Interfaces;
using OnlineTesting.Domain.Organizations;

namespace OnlineTesting.Application.Organizations.Branches.Commands.CreateBranch;

public class CreateBranchHandler : IRequestHandler<CreateBranchCommand, Guid>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUser _currentUser;

    public CreateBranchHandler(IApplicationDbContext db, ICurrentUser currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<Guid> Handle(CreateBranchCommand request, CancellationToken ct)
    {
        var organizationId = _currentUser.OrganizationId
            ?? throw new UnauthorizedException("User does not belong to an organization.");

        var branch = Branch.Create(organizationId, request.Name, request.Address);

        _db.Branches.Add(branch);
        await _db.SaveChangesAsync(ct);

        return branch.Id;
    }
}
