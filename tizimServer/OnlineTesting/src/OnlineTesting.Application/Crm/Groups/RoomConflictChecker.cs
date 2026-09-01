using Microsoft.EntityFrameworkCore;
using OnlineTesting.Application.Common.Interfaces;

namespace OnlineTesting.Application.Crm.Groups;

public static class RoomConflictChecker
{
    public static async Task<string?> FindConflictAsync(
        IApplicationDbContext db, Guid roomId, Guid excludeGroupId,
        IEnumerable<(DayOfWeek DayOfWeek, TimeOnly StartTime, TimeOnly EndTime)> slots, CancellationToken ct)
    {
        var otherSlots = await (
            from s in db.GroupScheduleSlots.AsNoTracking()
            join g in db.Groups.AsNoTracking() on s.GroupId equals g.Id
            where g.RoomId == roomId && g.Id != excludeGroupId
            select new { s.DayOfWeek, s.StartTime, s.EndTime, GroupName = g.Name }
        ).ToListAsync(ct);

        foreach (var slot in slots)
        {
            var conflict = otherSlots.FirstOrDefault(o =>
                o.DayOfWeek == slot.DayOfWeek &&
                slot.StartTime < o.EndTime && o.StartTime < slot.EndTime);

            if (conflict is not null)
                return $"Room is already booked by '{conflict.GroupName}' on {slot.DayOfWeek} at that time.";
        }

        return null;
    }
}
