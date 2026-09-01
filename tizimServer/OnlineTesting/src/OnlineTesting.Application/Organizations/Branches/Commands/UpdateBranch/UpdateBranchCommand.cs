using MediatR;

namespace OnlineTesting.Application.Organizations.Branches.Commands.UpdateBranch;

public record UpdateBranchCommand(Guid Id, string Name, string? Address) : IRequest;
