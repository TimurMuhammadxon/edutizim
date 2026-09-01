using MediatR;
using Microsoft.EntityFrameworkCore;
using OnlineTesting.Application.Common.Exceptions;
using OnlineTesting.Application.Common.Interfaces;
using OnlineTesting.Domain.Organizations;

namespace OnlineTesting.Application.Organizations.Rooms.Commands.CreateRoom;

public class CreateRoomHandler : IRequestHandler<CreateRoomCommand, Guid>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUser _currentUser;

    public CreateRoomHandler(IApplicationDbContext db, ICurrentUser currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<Guid> Handle(CreateRoomCommand request, CancellationToken ct)
    {
        var organizationId = _currentUser.OrganizationId
            ?? throw new UnauthorizedException("User does not belong to an organization.");

        var branchExists = await _db.Branches.AnyAsync(b => b.Id == request.BranchId, ct);
        if (!branchExists)
            throw new NotFoundException($"Branch '{request.BranchId}' not found.");

        var room = Room.Create(organizationId, request.BranchId, request.Name, request.Capacity);

        _db.Rooms.Add(room);
        await _db.SaveChangesAsync(ct);

        return room.Id;
    }
}
