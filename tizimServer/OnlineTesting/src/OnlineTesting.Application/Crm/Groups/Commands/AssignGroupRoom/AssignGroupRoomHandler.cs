using MediatR;
using Microsoft.EntityFrameworkCore;
using OnlineTesting.Application.Common.Exceptions;
using OnlineTesting.Application.Common.Interfaces;

namespace OnlineTesting.Application.Crm.Groups.Commands.AssignGroupRoom;

public class AssignGroupRoomHandler : IRequestHandler<AssignGroupRoomCommand>
{
    private readonly IApplicationDbContext _db;

    public AssignGroupRoomHandler(IApplicationDbContext db) => _db = db;

    public async Task Handle(AssignGroupRoomCommand request, CancellationToken ct)
    {
        var group = await _db.Groups.FirstOrDefaultAsync(g => g.Id == request.GroupId, ct)
            ?? throw new NotFoundException($"Group '{request.GroupId}' not found.");

        if (request.RoomId.HasValue)
        {
            var room = await _db.Rooms.FirstOrDefaultAsync(
                r => r.Id == request.RoomId.Value && r.OrganizationId == group.OrganizationId, ct)
                ?? throw new NotFoundException($"Room '{request.RoomId}' not found.");

            var slots = await _db.GroupScheduleSlots.AsNoTracking()
                .Where(s => s.GroupId == request.GroupId)
                .Select(s => new { s.DayOfWeek, s.StartTime, s.EndTime })
                .ToListAsync(ct);

            var conflict = await RoomConflictChecker.FindConflictAsync(
                _db, room.Id, request.GroupId,
                slots.Select(s => (s.DayOfWeek, s.StartTime, s.EndTime)), ct);
            if (conflict is not null)
                throw new ConflictException(conflict);
        }

        group.AssignRoom(request.RoomId);
        await _db.SaveChangesAsync(ct);
    }
}
