using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnlineTesting.Application.Common.Models;
using OnlineTesting.Application.Crm.Leads.Commands.AssignLeadManager;
using OnlineTesting.Application.Crm.Leads.Commands.ChangeLeadStage;
using OnlineTesting.Application.Crm.Leads.Commands.ConvertLeadToStudent;
using OnlineTesting.Application.Crm.Leads.Commands.CreateLead;
using OnlineTesting.Application.Crm.Leads.Commands.UpdateLead;
using OnlineTesting.Application.Crm.Leads.Queries.GetLeadById;
using OnlineTesting.Application.Crm.Leads.Queries.GetLeads;
using OnlineTesting.Domain.Authorization;
using OnlineTesting.Domain.Crm;

namespace OnlineTesting.API.Controllers.Crm;

[ApiController]
[Route("crm/leads")]
[Authorize(Policy = Roles.Policies.CrmAccess)]
public class LeadsController : ControllerBase
{
    private readonly ISender _sender;
    public LeadsController(ISender sender) => _sender = sender;

    [HttpGet]
    public Task<PagedResult<LeadDto>> List(
        [FromQuery] string? search,
        [FromQuery] ClientSource? source,
        [FromQuery] LeadStage? stage,
        [FromQuery] Guid? branchId,
        [FromQuery] Guid? assignedManagerId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
        => _sender.Send(new GetLeadsQuery(search, source, stage, branchId, assignedManagerId, page, pageSize), ct);

    [HttpGet("{id:guid}")]
    public Task<LeadDto> GetById(Guid id, CancellationToken ct)
        => _sender.Send(new GetLeadByIdQuery(id), ct);

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateLeadCommand cmd, CancellationToken ct)
    {
        var id = await _sender.Send(cmd, ct);
        return CreatedAtAction(nameof(GetById), new { id }, new { id });
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateLeadBody body, CancellationToken ct)
    {
        await _sender.Send(new UpdateLeadCommand(id, body.FullName, body.Phone, body.Email, body.Notes), ct);
        return NoContent();
    }

    [HttpPatch("{id:guid}/manager")]
    public async Task<IActionResult> AssignManager(Guid id, [FromBody] AssignManagerBody body, CancellationToken ct)
    {
        await _sender.Send(new AssignLeadManagerCommand(id, body.ManagerId), ct);
        return NoContent();
    }

    [HttpPatch("{id:guid}/stage")]
    public async Task<IActionResult> ChangeStage(Guid id, [FromBody] ChangeStageBody body, CancellationToken ct)
    {
        await _sender.Send(new ChangeLeadStageCommand(id, body.Stage, body.LostReason), ct);
        return NoContent();
    }

    [HttpPost("{id:guid}/convert")]
    public async Task<IActionResult> Convert(Guid id, CancellationToken ct)
    {
        var studentId = await _sender.Send(new ConvertLeadToStudentCommand(id), ct);
        return Ok(new { studentId });
    }

    public record UpdateLeadBody(string FullName, string Phone, string? Email, string? Notes);
    public record AssignManagerBody(Guid? ManagerId);
    public record ChangeStageBody(LeadStage Stage, string? LostReason);
}
