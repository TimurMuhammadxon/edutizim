using MediatR;
using Microsoft.EntityFrameworkCore;
using OnlineTesting.Application.Common.Exceptions;
using OnlineTesting.Application.Common.Interfaces;
using OnlineTesting.Domain.Users;

namespace OnlineTesting.Application.Crm.Attendance.Queries.GetGroupAttendance;

public class GetGroupAttendanceHandler : IRequestHandler<GetGroupAttendanceQuery, GroupAttendanceDto>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUser _currentUser;

    public GetGroupAttendanceHandler(IApplicationDbContext db, ICurrentUser currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<GroupAttendanceDto> Handle(GetGroupAttendanceQuery request, CancellationToken ct)
    {
        var group = await _db.Groups.AsNoTracking().FirstOrDefaultAsync(g => g.Id == request.GroupId, ct)
            ?? throw new NotFoundException($"Group '{request.GroupId}' not found.");

        if (_currentUser.Role == Role.Teacher && group.TeacherId != _currentUser.UserId)
            throw new NotFoundException($"Group '{request.GroupId}' not found.");

        var scheduledDays = await _db.GroupScheduleSlots.AsNoTracking()
            .Where(s => s.GroupId == request.GroupId)
            .Select(s => s.DayOfWeek)
            .Distinct()
            .ToListAsync(ct);

        var daysInMonth = DateTime.DaysInMonth(request.Year, request.Month);
        var lessonDates = Enumerable.Range(1, daysInMonth)
            .Select(day => new DateOnly(request.Year, request.Month, day))
            .Where(d => scheduledDays.Contains(d.DayOfWeek))
            .OrderBy(d => d)
            .ToList();

        var roster = await (
            from gs in _db.GroupStudents.AsNoTracking()
            join s in _db.Students.AsNoTracking() on gs.StudentId equals s.Id
            where gs.GroupId == request.GroupId
            select new { s.Id, s.FullName }
        ).ToListAsync(ct);

        var monthStart = new DateOnly(request.Year, request.Month, 1);
        var monthEnd = monthStart.AddMonths(1);
        var marks = await _db.Attendances.AsNoTracking()
            .Where(a => a.GroupId == request.GroupId && a.LessonDate >= monthStart && a.LessonDate < monthEnd)
            .ToListAsync(ct);

        var students = roster.Select(s => new AttendanceStudentRowDto(
            s.Id,
            s.FullName,
            marks.Where(m => m.StudentId == s.Id)
                .ToDictionary(m => m.LessonDate.ToString("yyyy-MM-dd"), m => m.Status)
        )).ToList();

        return new GroupAttendanceDto(lessonDates, students);
    }
}
