using MediatR;
using Microsoft.EntityFrameworkCore;
using OnlineTesting.Application.Common.Interfaces;

namespace OnlineTesting.Application.Organizations.Rooms.Queries.GetRooms;

public class GetRoomsHandler : IRequestHandler<GetRoomsQuery, List<RoomDto>>
{
    private readonly IApplicationDbContext _db;

    public GetRoomsHandler(IApplicationDbContext db) => _db = db;

    public async Task<List<RoomDto>> Handle(GetRoomsQuery request, CancellationToken ct)
    {
        var query = _db.Rooms.AsNoTracking().AsQueryable();

        if (request.BranchId.HasValue)
            query = query.Where(r => r.BranchId == request.BranchId.Value);

        if (request.IsActive.HasValue)
            query = query.Where(r => r.IsActive == request.IsActive.Value);

        return await query
            .OrderBy(r => r.Name)
            .Select(r => new RoomDto(r.Id, r.BranchId, r.Name, r.Capacity, r.IsActive, r.CreatedAt))
            .ToListAsync(ct);
    }
}
