using OnlineTesting.Domain.Common;

namespace OnlineTesting.Domain.Crm;

public class Attendance : Entity, ITenantScoped
{
    public Guid OrganizationId { get; private set; }
    public Guid GroupId { get; private set; }
    public Guid StudentId { get; private set; }
    public DateOnly LessonDate { get; private set; }
    public AttendanceStatus Status { get; private set; }
    public Guid MarkedByUserId { get; private set; }
    public DateTime MarkedAt { get; private set; }

    private Attendance() { }

    public static Attendance Create(
        Guid organizationId, Guid groupId, Guid studentId, DateOnly lessonDate,
        AttendanceStatus status, Guid markedByUserId)
    {
        return new Attendance
        {
            Id = Guid.NewGuid(),
            OrganizationId = organizationId,
            GroupId = groupId,
            StudentId = studentId,
            LessonDate = lessonDate,
            Status = status,
            MarkedByUserId = markedByUserId,
            MarkedAt = DateTime.UtcNow
        };
    }

    public void ChangeStatus(AttendanceStatus status, Guid markedByUserId)
    {
        Status = status;
        MarkedByUserId = markedByUserId;
        MarkedAt = DateTime.UtcNow;
    }
}
