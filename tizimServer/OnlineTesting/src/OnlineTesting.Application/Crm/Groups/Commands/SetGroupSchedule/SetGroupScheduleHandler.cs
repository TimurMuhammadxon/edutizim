using MediatR;
using Microsoft.EntityFrameworkCore;
using OnlineTesting.Application.Common.Exceptions;
using OnlineTesting.Application.Common.Interfaces;
using OnlineTesting.Domain.Crm;

namespace OnlineTesting.Application.Crm.Groups.Commands.SetGroupSchedule;

public class SetGroupScheduleHandler : IRequestHandler<SetGroupScheduleCommand>
{
    private readonly IApplicationDbContext _db;

    public SetGroupScheduleHandler(IApplicationDbContext db) => _db = db;

    public async Task Handle(SetGroupScheduleCommand request, CancellationToken ct)
    {
        var group = await _db.Groups.FirstOrDefaultAsync(g => g.Id == request.GroupId, ct)
            ?? throw new NotFoundException($"Group '{request.GroupId}' not found.");

        if (group.RoomId.HasValue)
        {
            var conflict = await RoomConflictChecker.FindConflictAsync(
                _db, group.RoomId.Value, request.GroupId,
                request.Slots.Select(s => (s.DayOfWeek, s.StartTime, s.EndTime)), ct);
            if (conflict is not null)
                throw new ConflictException(conflict);
        }

        var existing = await _db.GroupScheduleSlots.Where(s => s.GroupId == request.GroupId).ToListAsync(ct);
        _db.GroupScheduleSlots.RemoveRange(existing);

        foreach (var slot in request.Slots)
        {
            _db.GroupScheduleSlots.Add(GroupScheduleSlot.Create(
                group.OrganizationId, group.Id, slot.DayOfWeek, slot.StartTime, slot.EndTime));
        }

        await _db.SaveChangesAsync(ct);
    }
}
