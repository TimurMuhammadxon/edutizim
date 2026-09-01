using MediatR;
using OnlineTesting.Application.Organizations.Branches.Queries.GetBranches;

namespace OnlineTesting.Application.Organizations.Branches.Queries.GetBranchById;

public record GetBranchByIdQuery(Guid Id) : IRequest<BranchDto>;
