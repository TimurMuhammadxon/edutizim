using MediatR;
using OnlineTesting.Domain.Crm;

namespace OnlineTesting.Application.Crm.Attendance.Queries.GetGroupAttendance;

public record GetGroupAttendanceQuery(Guid GroupId, int Year, int Month) : IRequest<GroupAttendanceDto>;

public record GroupAttendanceDto(List<DateOnly> LessonDates, List<AttendanceStudentRowDto> Students);

public record AttendanceStudentRowDto(Guid StudentId, string FullName, Dictionary<string, AttendanceStatus> Marks);
