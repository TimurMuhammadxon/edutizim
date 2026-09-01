using MediatR;
using Microsoft.EntityFrameworkCore;
using OnlineTesting.Application.Common.Exceptions;
using OnlineTesting.Application.Common.Interfaces;
using OnlineTesting.Domain.Users;

namespace OnlineTesting.Application.Crm.Attendance.Commands.SetAttendanceForDate;

public class SetAttendanceForDateHandler : IRequestHandler<SetAttendanceForDateCommand>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUser _currentUser;

    public SetAttendanceForDateHandler(IApplicationDbContext db, ICurrentUser currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task Handle(SetAttendanceForDateCommand request, CancellationToken ct)
    {
        var group = await _db.Groups.FirstOrDefaultAsync(g => g.Id == request.GroupId, ct)
            ?? throw new NotFoundException($"Group '{request.GroupId}' not found.");

        if (_currentUser.Role == Role.Teacher && group.TeacherId != _currentUser.UserId)
            throw new NotFoundException($"Group '{request.GroupId}' not found.");

        var scheduledDay = await _db.GroupScheduleSlots
            .AnyAsync(s => s.GroupId == request.GroupId && s.DayOfWeek == request.LessonDate.DayOfWeek, ct);
        if (!scheduledDay)
            throw new ConflictException("Lesson date does not match the group's schedule.");

        var studentIds = await _db.GroupStudents
            .Where(gs => gs.GroupId == request.GroupId)
            .Select(gs => gs.StudentId)
            .ToListAsync(ct);

        var existingByStudent = await _db.Attendances
            .Where(a => a.GroupId == request.GroupId && a.LessonDate == request.LessonDate)
            .ToDictionaryAsync(a => a.StudentId, ct);

        foreach (var studentId in studentIds)
        {
            existingByStudent.TryGetValue(studentId, out var mark);

            if (request.Status is null)
            {
                if (mark is not null)
                    _db.Attendances.Remove(mark);
            }
            else if (mark is not null)
            {
                mark.ChangeStatus(request.Status.Value, _currentUser.UserId!.Value);
            }
            else
            {
                _db.Attendances.Add(Domain.Crm.Attendance.Create(
                    group.OrganizationId, request.GroupId, studentId, request.LessonDate,
                    request.Status.Value, _currentUser.UserId!.Value));
            }
        }

        await _db.SaveChangesAsync(ct);
    }
}
