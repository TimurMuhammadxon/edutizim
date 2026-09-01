using MediatR;

namespace OnlineTesting.Application.Crm.Groups.Commands.AssignGroupTeacher;

public record AssignGroupTeacherCommand(Guid GroupId, Guid? TeacherId) : IRequest;
