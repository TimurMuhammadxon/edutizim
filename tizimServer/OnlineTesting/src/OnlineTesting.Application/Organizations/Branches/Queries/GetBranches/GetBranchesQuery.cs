using MediatR;

namespace OnlineTesting.Application.Organizations.Branches.Queries.GetBranches;

public record GetBranchesQuery(bool? IsActive) : IRequest<List<BranchDto>>;

public record BranchDto(Guid Id, string Name, string? Address, bool IsActive, DateTime CreatedAt);
