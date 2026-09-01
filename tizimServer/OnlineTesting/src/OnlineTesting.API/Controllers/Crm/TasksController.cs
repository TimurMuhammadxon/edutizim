using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnlineTesting.Application.Common.Models;
using OnlineTesting.Application.Crm.Tasks.Commands.CancelTask;
using OnlineTesting.Application.Crm.Tasks.Commands.CompleteTask;
using OnlineTesting.Application.Crm.Tasks.Commands.CreateTask;
using OnlineTesting.Application.Crm.Tasks.Commands.RescheduleTask;
using OnlineTesting.Application.Crm.Tasks.Queries.GetTaskById;
using OnlineTesting.Application.Crm.Tasks.Queries.GetTasks;
using OnlineTesting.Domain.Authorization;
using OnlineTesting.Domain.Crm;

namespace OnlineTesting.API.Controllers.Crm;

[ApiController]
[Route("crm/tasks")]
[Authorize(Policy = Roles.Policies.CrmAccess)]
public class TasksController : ControllerBase
{
    private readonly ISender _sender;
    public TasksController(ISender sender) => _sender = sender;

    [HttpGet]
    public Task<PagedResult<CrmTaskDto>> List(
        [FromQuery] CrmTaskStatus? status,
        [FromQuery] Guid? assignedToUserId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50,
        CancellationToken ct = default)
        => _sender.Send(new GetTasksQuery(status, assignedToUserId, page, pageSize), ct);

    [HttpGet("{id:guid}")]
    public Task<CrmTaskDto> GetById(Guid id, CancellationToken ct)
        => _sender.Send(new GetTaskByIdQuery(id), ct);

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateTaskCommand cmd, CancellationToken ct)
    {
        var id = await _sender.Send(cmd, ct);
        return CreatedAtAction(nameof(GetById), new { id }, new { id });
    }

    [HttpPost("{id:guid}/complete")]
    public async Task<IActionResult> Complete(Guid id, CancellationToken ct)
    {
        await _sender.Send(new CompleteTaskCommand(id), ct);
        return NoContent();
    }

    [HttpPost("{id:guid}/cancel")]
    public async Task<IActionResult> Cancel(Guid id, CancellationToken ct)
    {
        await _sender.Send(new CancelTaskCommand(id), ct);
        return NoContent();
    }

    [HttpPatch("{id:guid}/reschedule")]
    public async Task<IActionResult> Reschedule(Guid id, [FromBody] RescheduleBody body, CancellationToken ct)
    {
        await _sender.Send(new RescheduleTaskCommand(id, body.DueAt), ct);
        return NoContent();
    }

    public record RescheduleBody(DateTime DueAt);
}
