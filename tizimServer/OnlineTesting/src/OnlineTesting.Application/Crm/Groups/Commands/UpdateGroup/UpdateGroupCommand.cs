using MediatR;

namespace OnlineTesting.Application.Crm.Groups.Commands.UpdateGroup;

public record UpdateGroupCommand(Guid Id, string Name, decimal Price, string? Description) : IRequest;
