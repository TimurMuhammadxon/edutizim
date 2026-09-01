using MediatR;

namespace OnlineTesting.Application.Organizations.Members.Commands.CreateTeacherMember;

public record CreateTeacherMemberCommand(string Phone, string Password, string? FirstName, string? LastName) : IRequest<Guid>;
