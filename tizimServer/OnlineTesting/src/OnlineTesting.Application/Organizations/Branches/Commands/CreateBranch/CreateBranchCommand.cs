using MediatR;

namespace OnlineTesting.Application.Organizations.Branches.Commands.CreateBranch;

public record CreateBranchCommand(string Name, string? Address) : IRequest<Guid>;
