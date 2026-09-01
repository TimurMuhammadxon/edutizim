using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnlineTesting.Application.Organizations.Rooms.Commands.CreateRoom;
using OnlineTesting.Application.Organizations.Rooms.Commands.ToggleRoomActive;
using OnlineTesting.Application.Organizations.Rooms.Commands.UpdateRoom;
using OnlineTesting.Application.Organizations.Rooms.Queries.GetRooms;
using OnlineTesting.Domain.Authorization;

namespace OnlineTesting.API.Controllers.Organizations;

[ApiController]
[Route("org/rooms")]
public class RoomsController : ControllerBase
{
    private readonly ISender _sender;
    public RoomsController(ISender sender) => _sender = sender;

    [HttpGet]
    [Authorize(Policy = Roles.Policies.CrmAccess)]
    public Task<List<RoomDto>> List([FromQuery] Guid? branchId, [FromQuery] bool? isActive, CancellationToken ct)
        => _sender.Send(new GetRoomsQuery(branchId, isActive), ct);

    [HttpPost]
    [Authorize(Policy = Roles.Policies.OrgAdminAccess)]
    public async Task<IActionResult> Create([FromBody] CreateRoomCommand cmd, CancellationToken ct)
    {
        var id = await _sender.Send(cmd, ct);
        return Ok(new { id });
    }

    [HttpPut("{id:guid}")]
    [Authorize(Policy = Roles.Policies.OrgAdminAccess)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateRoomBody body, CancellationToken ct)
    {
        await _sender.Send(new UpdateRoomCommand(id, body.Name, body.Capacity), ct);
        return NoContent();
    }

    [HttpPatch("{id:guid}/active")]
    [Authorize(Policy = Roles.Policies.OrgAdminAccess)]
    public async Task<IActionResult> ToggleActive(Guid id, [FromBody] ToggleActiveBody body, CancellationToken ct)
    {
        await _sender.Send(new ToggleRoomActiveCommand(id, body.IsActive), ct);
        return NoContent();
    }

    public record UpdateRoomBody(string Name, int Capacity);
    public record ToggleActiveBody(bool IsActive);
}
