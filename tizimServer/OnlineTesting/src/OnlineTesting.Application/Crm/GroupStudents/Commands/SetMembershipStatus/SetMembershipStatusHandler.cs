using MediatR;
using Microsoft.EntityFrameworkCore;
using OnlineTesting.Application.Common.Exceptions;
using OnlineTesting.Application.Common.Interfaces;
using OnlineTesting.Domain.Crm;

namespace OnlineTesting.Application.Crm.GroupStudents.Commands.SetMembershipStatus;

public class SetMembershipStatusHandler : IRequestHandler<SetMembershipStatusCommand>
{
    private readonly IApplicationDbContext _db;

    public SetMembershipStatusHandler(IApplicationDbContext db) => _db = db;

    public async Task Handle(SetMembershipStatusCommand request, CancellationToken ct)
    {
        var membership = await _db.GroupStudents.FirstOrDefaultAsync(
            gs => gs.GroupId == request.GroupId && gs.StudentId == request.StudentId, ct)
            ?? throw new NotFoundException($"Student '{request.StudentId}' is not a member of this group.");

        switch (request.Status)
        {
            case GroupMembershipStatus.Frozen:
                membership.Freeze();
                break;
            case GroupMembershipStatus.Active:
                membership.Unfreeze();
                break;
            case GroupMembershipStatus.Left:
                membership.MarkLeft();
                break;
            default:
                throw new ConflictException("Trial status can only be set by adding a new member; it cannot be set directly.");
        }

        await _db.SaveChangesAsync(ct);
    }
}
