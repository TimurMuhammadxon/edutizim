using MediatR;
using Microsoft.EntityFrameworkCore;
using OnlineTesting.Application.Common.Exceptions;
using OnlineTesting.Application.Common.Interfaces;
using OnlineTesting.Domain.Crm;

namespace OnlineTesting.Application.Crm.Students.Queries.GetStudentAttendance;

public class GetStudentAttendanceHandler : IRequestHandler<GetStudentAttendanceQuery, StudentAttendanceDto>
{
    private readonly IApplicationDbContext _db;

    public GetStudentAttendanceHandler(IApplicationDbContext db) => _db = db;

    public async Task<StudentAttendanceDto> Handle(GetStudentAttendanceQuery request, CancellationToken ct)
    {
        var exists = await _db.Students.AsNoTracking().AnyAsync(s => s.Id == request.StudentId, ct);
        if (!exists)
            throw new NotFoundException($"Student '{request.StudentId}' not found.");

        var memberships = await (
            from gs in _db.GroupStudents.AsNoTracking()
            join g in _db.Groups.AsNoTracking() on gs.GroupId equals g.Id
            where gs.StudentId == request.StudentId
            select new { g.Id, g.Name }
        ).ToListAsync(ct);

        var groupIds = memberships.Select(m => m.Id).ToList();

        var scheduleByGroup = await _db.GroupScheduleSlots.AsNoTracking()
            .Where(s => groupIds.Contains(s.GroupId))
            .Select(s => new { s.GroupId, s.DayOfWeek })
            .ToListAsync(ct);
        var scheduleLookup = scheduleByGroup.ToLookup(s => s.GroupId, s => s.DayOfWeek);

        var daysInMonth = DateTime.DaysInMonth(request.Year, request.Month);
        var monthStart = new DateOnly(request.Year, request.Month, 1);
        var monthEnd = monthStart.AddMonths(1);

        var marksRaw = await _db.Attendances.AsNoTracking()
            .Where(a => a.StudentId == request.StudentId && groupIds.Contains(a.GroupId)
                && a.LessonDate >= monthStart && a.LessonDate < monthEnd)
            .ToListAsync(ct);
        var marksByGroup = marksRaw.ToLookup(a => a.GroupId);

        var groups = memberships.Select(m =>
        {
            var scheduledDays = scheduleLookup[m.Id].Distinct().ToHashSet();
            var lessonDates = Enumerable.Range(1, daysInMonth)
                .Select(day => new DateOnly(request.Year, request.Month, day))
                .Where(d => scheduledDays.Contains(d.DayOfWeek))
                .OrderBy(d => d)
                .ToList();

            var groupMarks = marksByGroup[m.Id].ToDictionary(a => a.LessonDate.ToString("yyyy-MM-dd"), a => a.Status);
            var presentCount = groupMarks.Values.Count(s => s == AttendanceStatus.Present);
            var absentCount = groupMarks.Values.Count(s => s == AttendanceStatus.Absent);

            return new StudentAttendanceGroupDto(m.Id, m.Name, lessonDates, groupMarks, presentCount, absentCount);
        }).ToList();

        return new StudentAttendanceDto(groups);
    }
}
