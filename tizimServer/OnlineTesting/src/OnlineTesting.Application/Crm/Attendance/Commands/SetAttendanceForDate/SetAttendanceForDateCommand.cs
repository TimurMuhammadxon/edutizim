using MediatR;
using OnlineTesting.Domain.Crm;

namespace OnlineTesting.Application.Crm.Attendance.Commands.SetAttendanceForDate;

public record SetAttendanceForDateCommand(Guid GroupId, DateOnly LessonDate, AttendanceStatus? Status) : IRequest;
