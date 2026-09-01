using MediatR;

namespace OnlineTesting.Application.Organizations.Branches.Commands.ToggleBranchActive;

public record ToggleBranchActiveCommand(Guid Id, bool IsActive) : IRequest;
