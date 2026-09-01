using MediatR;
using Microsoft.EntityFrameworkCore;
using OnlineTesting.Application.Common.Exceptions;
using OnlineTesting.Application.Common.Interfaces;

namespace OnlineTesting.Application.Organizations.Rooms.Commands.ToggleRoomActive;

public class ToggleRoomActiveHandler : IRequestHandler<ToggleRoomActiveCommand>
{
    private readonly IApplicationDbContext _db;

    public ToggleRoomActiveHandler(IApplicationDbContext db) => _db = db;

    public async Task Handle(ToggleRoomActiveCommand request, CancellationToken ct)
    {
        var room = await _db.Rooms.FirstOrDefaultAsync(r => r.Id == request.Id, ct)
            ?? throw new NotFoundException($"Room '{request.Id}' not found.");

        if (request.IsActive) room.Activate();
        else room.Deactivate();

        await _db.SaveChangesAsync(ct);
    }
}
