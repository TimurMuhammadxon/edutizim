using MediatR;

namespace OnlineTesting.Application.Crm.Groups.Commands.CreateGroup;

public record CreateGroupCommand(Guid BranchId, string Name, decimal Price, string? Description) : IRequest<Guid>;
