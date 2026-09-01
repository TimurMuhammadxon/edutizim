using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnlineTesting.Application.Common.Models;
using OnlineTesting.Application.Crm.Students.Commands.CreateStudent;
using OnlineTesting.Application.Crm.Students.Commands.CreateStudentLogin;
using OnlineTesting.Application.Crm.Students.Commands.ToggleStudentActive;
using OnlineTesting.Application.Crm.Students.Commands.UpdateStudent;
using OnlineTesting.Application.Crm.Students.Queries.GetStudentAttendance;
using OnlineTesting.Application.Crm.Students.Queries.GetStudentById;
using OnlineTesting.Application.Crm.Students.Queries.GetStudents;
using OnlineTesting.Domain.Authorization;

namespace OnlineTesting.API.Controllers.Crm;

[ApiController]
[Route("crm/students")]
[Authorize(Policy = Roles.Policies.CrmAccess)]
public class StudentsController : ControllerBase
{
    private readonly ISender _sender;
    public StudentsController(ISender sender) => _sender = sender;

    [HttpGet]
    public Task<PagedResult<StudentDto>> List(
        [FromQuery] string? search,
        [FromQuery] Guid? branchId,
        [FromQuery] Guid? groupId,
        [FromQuery] bool? isActive,
        [FromQuery] string? studentStatus,
        [FromQuery] string? financialStatus,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
        => _sender.Send(new GetStudentsQuery(search, branchId, groupId, isActive, studentStatus, financialStatus, page, pageSize), ct);

    [HttpGet("{id:guid}")]
    public Task<StudentDetailsDto> GetById(Guid id, CancellationToken ct)
        => _sender.Send(new GetStudentByIdQuery(id), ct);

    [HttpGet("{id:guid}/attendance")]
    public Task<StudentAttendanceDto> GetAttendance(Guid id, [FromQuery] int year, [FromQuery] int month, CancellationToken ct)
        => _sender.Send(new GetStudentAttendanceQuery(id, year, month), ct);

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateStudentCommand cmd, CancellationToken ct)
    {
        var id = await _sender.Send(cmd, ct);
        return CreatedAtAction(nameof(GetById), new { id }, new { id });
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateStudentBody body, CancellationToken ct)
    {
        await _sender.Send(new UpdateStudentCommand(id, body.FullName, body.Phone, body.Email, body.Notes), ct);
        return NoContent();
    }

    [HttpPatch("{id:guid}/active")]
    public async Task<IActionResult> ToggleActive(Guid id, [FromBody] ToggleActiveBody body, CancellationToken ct)
    {
        await _sender.Send(new ToggleStudentActiveCommand(id, body.IsActive), ct);
        return NoContent();
    }

    [HttpPost("{id:guid}/login")]
    public async Task<IActionResult> CreateLogin(Guid id, [FromBody] CreateLoginBody body, CancellationToken ct)
    {
        var userId = await _sender.Send(new CreateStudentLoginCommand(id, body.Password), ct);
        return Ok(new { userId });
    }

    public record UpdateStudentBody(string FullName, string Phone, string? Email, string? Notes);
    public record ToggleActiveBody(bool IsActive);
    public record CreateLoginBody(string Password);
}
