using MediatR;

namespace OnlineTesting.Application.Organizations.Members.Commands.CreateStaffMember;

public record CreateStaffMemberCommand(string Phone, string Password, string? FirstName, string? LastName) : IRequest<Guid>;
