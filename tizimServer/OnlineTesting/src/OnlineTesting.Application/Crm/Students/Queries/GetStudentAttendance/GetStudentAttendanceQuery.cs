using MediatR;
using OnlineTesting.Domain.Crm;

namespace OnlineTesting.Application.Crm.Students.Queries.GetStudentAttendance;

public record GetStudentAttendanceQuery(Guid StudentId, int Year, int Month) : IRequest<StudentAttendanceDto>;

public record StudentAttendanceDto(List<StudentAttendanceGroupDto> Groups);

public record StudentAttendanceGroupDto(
    Guid GroupId,
    string GroupName,
    List<DateOnly> LessonDates,
    Dictionary<string, AttendanceStatus> Marks,
    int PresentCount,
    int AbsentCount);
