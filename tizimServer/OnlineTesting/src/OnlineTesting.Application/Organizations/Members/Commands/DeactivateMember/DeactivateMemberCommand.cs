using MediatR;

namespace OnlineTesting.Application.Organizations.Members.Commands.DeactivateMember;

public record DeactivateMemberCommand(Guid Id) : IRequest;
