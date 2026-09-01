using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnlineTesting.Application.Organizations.Branches.Commands.CreateBranch;
using OnlineTesting.Application.Organizations.Branches.Commands.ToggleBranchActive;
using OnlineTesting.Application.Organizations.Branches.Commands.UpdateBranch;
using OnlineTesting.Application.Organizations.Branches.Queries.GetBranchById;
using OnlineTesting.Application.Organizations.Branches.Queries.GetBranches;
using OnlineTesting.Domain.Authorization;

namespace OnlineTesting.API.Controllers.Organizations;

[ApiController]
[Route("org/branches")]
public class BranchesController : ControllerBase
{
    private readonly ISender _sender;
    public BranchesController(ISender sender) => _sender = sender;

    // Reading the branch list is needed by CRM staff too (to scope leads/students to a
    // branch, and to populate the branch switcher) — only managing branches is OrgAdmin-only.
    [HttpGet]
    [Authorize(Policy = Roles.Policies.CrmAccess)]
    public Task<List<BranchDto>> List([FromQuery] bool? isActive, CancellationToken ct)
        => _sender.Send(new GetBranchesQuery(isActive), ct);

    [HttpGet("{id:guid}")]
    [Authorize(Policy = Roles.Policies.CrmAccess)]
    public Task<BranchDto> GetById(Guid id, CancellationToken ct)
        => _sender.Send(new GetBranchByIdQuery(id), ct);

    [HttpPost]
    [Authorize(Policy = Roles.Policies.OrgAdminAccess)]
    public async Task<IActionResult> Create([FromBody] CreateBranchCommand cmd, CancellationToken ct)
    {
        var id = await _sender.Send(cmd, ct);
        return CreatedAtAction(nameof(GetById), new { id }, new { id });
    }

    [HttpPut("{id:guid}")]
    [Authorize(Policy = Roles.Policies.OrgAdminAccess)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateBranchBody body, CancellationToken ct)
    {
        await _sender.Send(new UpdateBranchCommand(id, body.Name, body.Address), ct);
        return NoContent();
    }

    [HttpPatch("{id:guid}/active")]
    [Authorize(Policy = Roles.Policies.OrgAdminAccess)]
    public async Task<IActionResult> ToggleActive(Guid id, [FromBody] ToggleActiveBody body, CancellationToken ct)
    {
        await _sender.Send(new ToggleBranchActiveCommand(id, body.IsActive), ct);
        return NoContent();
    }

    public record UpdateBranchBody(string Name, string? Address);
    public record ToggleActiveBody(bool IsActive);
}
