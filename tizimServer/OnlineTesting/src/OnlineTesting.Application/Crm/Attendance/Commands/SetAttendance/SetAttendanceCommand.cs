using MediatR;
using OnlineTesting.Domain.Crm;

namespace OnlineTesting.Application.Crm.Attendance.Commands.SetAttendance;

public record SetAttendanceCommand(Guid GroupId, Guid StudentId, DateOnly LessonDate, AttendanceStatus? Status) : IRequest;
