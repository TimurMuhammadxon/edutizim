using OnlineTesting.Domain.Common;

namespace OnlineTesting.Domain.Crm;

public class GroupScheduleSlot : Entity, ITenantScoped
{
    public Guid OrganizationId { get; private set; }
    public Guid GroupId { get; private set; }
    public DayOfWeek DayOfWeek { get; private set; }
    public TimeOnly StartTime { get; private set; }
    public TimeOnly EndTime { get; private set; }

    private GroupScheduleSlot() { }

    public static GroupScheduleSlot Create(
        Guid organizationId, Guid groupId, DayOfWeek dayOfWeek, TimeOnly startTime, TimeOnly endTime)
    {
        if (endTime <= startTime)
            throw new ArgumentException("End time must be after start time.", nameof(endTime));

        return new GroupScheduleSlot
        {
            Id = Guid.NewGuid(),
            OrganizationId = organizationId,
            GroupId = groupId,
            DayOfWeek = dayOfWeek,
            StartTime = startTime,
            EndTime = endTime
        };
    }
}
