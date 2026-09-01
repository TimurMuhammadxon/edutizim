using MediatR;
using Microsoft.EntityFrameworkCore;
using OnlineTesting.Application.Common.Exceptions;
using OnlineTesting.Application.Common.Interfaces;
using OnlineTesting.Domain.Crm;

namespace OnlineTesting.Application.Crm.Groups.Commands.CreateGroup;

public class CreateGroupHandler : IRequestHandler<CreateGroupCommand, Guid>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUser _currentUser;

    public CreateGroupHandler(IApplicationDbContext db, ICurrentUser currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<Guid> Handle(CreateGroupCommand request, CancellationToken ct)
    {
        var organizationId = _currentUser.OrganizationId
            ?? throw new UnauthorizedException("User does not belong to an organization.");

        var branchExists = await _db.Branches.AnyAsync(b => b.Id == request.BranchId, ct);
        if (!branchExists)
            throw new NotFoundException($"Branch '{request.BranchId}' not found.");

        var group = Group.Create(organizationId, request.BranchId, request.Name, request.Price, request.Description);

        _db.Groups.Add(group);
        await _db.SaveChangesAsync(ct);

        return group.Id;
    }
}
