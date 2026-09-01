using MediatR;

namespace OnlineTesting.Application.Crm.Groups.Commands.AssignGroupRoom;

public record AssignGroupRoomCommand(Guid GroupId, Guid? RoomId) : IRequest;
