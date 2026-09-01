using MediatR;
using Microsoft.EntityFrameworkCore;
using OnlineTesting.Application.Common.Exceptions;
using OnlineTesting.Application.Common.Interfaces;

namespace OnlineTesting.Application.Organizations.Members.Commands.DeactivateMember;

public class DeactivateMemberHandler : IRequestHandler<DeactivateMemberCommand>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUser _currentUser;

    public DeactivateMemberHandler(IApplicationDbContext db, ICurrentUser currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task Handle(DeactivateMemberCommand request, CancellationToken ct)
    {
        var organizationId = _currentUser.OrganizationId
            ?? throw new UnauthorizedException("User does not belong to an organization.");

        var member = await _db.Users
            .FirstOrDefaultAsync(u => u.Id == request.Id && u.OrganizationId == organizationId, ct)
            ?? throw new NotFoundException($"Member '{request.Id}' not found.");

        member.Deactivate();
        await _db.SaveChangesAsync(ct);
    }
}
