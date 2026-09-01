using MediatR;

namespace OnlineTesting.Application.Crm.Groups.Commands.AddStudentToGroup;

public record AddStudentToGroupCommand(Guid GroupId, Guid StudentId) : IRequest;
