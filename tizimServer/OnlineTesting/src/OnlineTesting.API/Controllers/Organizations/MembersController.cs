using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnlineTesting.Application.Organizations.Members.Commands.CreateStaffMember;
using OnlineTesting.Application.Organizations.Members.Commands.CreateTeacherMember;
using OnlineTesting.Application.Organizations.Members.Commands.DeactivateMember;
using OnlineTesting.Application.Organizations.Members.Queries.GetMembers;
using OnlineTesting.Domain.Authorization;
using OnlineTesting.Domain.Users;

namespace OnlineTesting.API.Controllers.Organizations;

[ApiController]
[Route("org/members")]
public class MembersController : ControllerBase
{
    private readonly ISender _sender;
    public MembersController(ISender sender) => _sender = sender;

    [HttpGet]
    [Authorize(Policy = Roles.Policies.CrmAccess)]
    public Task<List<MemberDto>> List([FromQuery] Role? role, CancellationToken ct)
        => _sender.Send(new GetMembersQuery(role), ct);

    [HttpPost("staff")]
    [Authorize(Policy = Roles.Policies.OrgAdminAccess)]
    public async Task<IActionResult> CreateStaff([FromBody] CreateStaffMemberCommand cmd, CancellationToken ct)
    {
        var id = await _sender.Send(cmd, ct);
        return CreatedAtAction(nameof(List), new { id });
    }

    [HttpPost("teachers")]
    [Authorize(Policy = Roles.Policies.CrmAccess)]
    public async Task<IActionResult> CreateTeacher([FromBody] CreateTeacherMemberCommand cmd, CancellationToken ct)
    {
        var id = await _sender.Send(cmd, ct);
        return CreatedAtAction(nameof(List), new { id });
    }

    [HttpPatch("{id:guid}/deactivate")]
    [Authorize(Policy = Roles.Policies.OrgAdminAccess)]
    public async Task<IActionResult> Deactivate(Guid id, CancellationToken ct)
    {
        await _sender.Send(new DeactivateMemberCommand(id), ct);
        return NoContent();
    }
}
