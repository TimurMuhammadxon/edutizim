using MediatR;

namespace OnlineTesting.Application.Organizations.Rooms.Queries.GetRooms;

public record GetRoomsQuery(Guid? BranchId, bool? IsActive) : IRequest<List<RoomDto>>;

public record RoomDto(Guid Id, Guid BranchId, string Name, int Capacity, bool IsActive, DateTime CreatedAt);
