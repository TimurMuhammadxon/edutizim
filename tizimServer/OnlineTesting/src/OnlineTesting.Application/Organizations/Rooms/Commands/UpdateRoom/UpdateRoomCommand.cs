using MediatR;

namespace OnlineTesting.Application.Organizations.Rooms.Commands.UpdateRoom;

public record UpdateRoomCommand(Guid Id, string Name, int Capacity) : IRequest;
