using MediatR;

namespace OnlineTesting.Application.Crm.Groups.Commands.RemoveStudentFromGroup;

public record RemoveStudentFromGroupCommand(Guid GroupId, Guid StudentId) : IRequest;
