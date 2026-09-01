using MediatR;
using Microsoft.EntityFrameworkCore;
using OnlineTesting.Application.Common.Exceptions;
using OnlineTesting.Application.Common.Interfaces;
using OnlineTesting.Domain.Crm;
using OnlineTesting.Domain.Users;

namespace OnlineTesting.Application.Crm.Attendance.Commands.SetAttendance;

public class SetAttendanceHandler : IRequestHandler<SetAttendanceCommand>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUser _currentUser;

    public SetAttendanceHandler(IApplicationDbContext db, ICurrentUser currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task Handle(SetAttendanceCommand request, CancellationToken ct)
    {
        var group = await _db.Groups.FirstOrDefaultAsync(g => g.Id == request.GroupId, ct)
            ?? throw new NotFoundException($"Group '{request.GroupId}' not found.");

        if (_currentUser.Role == Role.Teacher && group.TeacherId != _currentUser.UserId)
            throw new NotFoundException($"Group '{request.GroupId}' not found.");

        var isMember = await _db.GroupStudents
            .AnyAsync(gs => gs.GroupId == request.GroupId && gs.StudentId == request.StudentId, ct);
        if (!isMember)
            throw new NotFoundException($"Student '{request.StudentId}' is not a member of this group.");

        var scheduledDay = await _db.GroupScheduleSlots
            .AnyAsync(s => s.GroupId == request.GroupId && s.DayOfWeek == request.LessonDate.DayOfWeek, ct);
        if (!scheduledDay)
            throw new ConflictException("Lesson date does not match the group's schedule.");

        var existing = await _db.Attendances.FirstOrDefaultAsync(
            a => a.GroupId == request.GroupId && a.StudentId == request.StudentId && a.LessonDate == request.LessonDate, ct);

        if (request.Status is null)
        {
            if (existing is not null)
                _db.Attendances.Remove(existing);
        }
        else if (existing is not null)
        {
            existing.ChangeStatus(request.Status.Value, _currentUser.UserId!.Value);
        }
        else
        {
            _db.Attendances.Add(Domain.Crm.Attendance.Create(
                group.OrganizationId, request.GroupId, request.StudentId, request.LessonDate,
                request.Status.Value, _currentUser.UserId!.Value));
        }

        await _db.SaveChangesAsync(ct);
    }
}
