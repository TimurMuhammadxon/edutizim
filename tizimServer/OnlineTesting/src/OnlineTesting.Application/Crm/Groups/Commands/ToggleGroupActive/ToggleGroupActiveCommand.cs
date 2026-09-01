using MediatR;

namespace OnlineTesting.Application.Crm.Groups.Commands.ToggleGroupActive;

public record ToggleGroupActiveCommand(Guid Id, bool IsActive) : IRequest;
