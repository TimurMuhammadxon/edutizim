using MediatR;

namespace OnlineTesting.Application.Organizations.Rooms.Commands.ToggleRoomActive;

public record ToggleRoomActiveCommand(Guid Id, bool IsActive) : IRequest;
