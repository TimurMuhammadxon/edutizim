using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnlineTesting.Application.Common.Models;
using OnlineTesting.Application.Crm.Attendance.Commands.SetAttendance;
using OnlineTesting.Application.Crm.Attendance.Commands.SetAttendanceForDate;
using OnlineTesting.Application.Crm.Attendance.Queries.GetGroupAttendance;
using OnlineTesting.Application.Crm.GroupStudents.Commands.RemoveDiscount;
using OnlineTesting.Application.Crm.GroupStudents.Commands.SetDiscount;
using OnlineTesting.Application.Crm.GroupStudents.Commands.SetMembershipStatus;
using OnlineTesting.Application.Crm.Groups.Commands.AddStudentToGroup;
using OnlineTesting.Application.Crm.Groups.Commands.AssignGroupRoom;
using OnlineTesting.Application.Crm.Groups.Commands.AssignGroupTeacher;
using OnlineTesting.Application.Crm.Groups.Commands.CreateGroup;
using OnlineTesting.Application.Crm.Groups.Commands.RemoveStudentFromGroup;
using OnlineTesting.Application.Crm.Groups.Commands.SetGroupSchedule;
using OnlineTesting.Application.Crm.Groups.Commands.ToggleGroupActive;
using OnlineTesting.Application.Crm.Groups.Commands.UpdateGroup;
using OnlineTesting.Application.Crm.Groups.Queries.GetGroupById;
using OnlineTesting.Application.Crm.Groups.Queries.GetGroups;
using OnlineTesting.Domain.Authorization;
using OnlineTesting.Domain.Crm;

namespace OnlineTesting.API.Controllers.Crm;

[ApiController]
[Route("crm/groups")]
public class GroupsController : ControllerBase
{
    private readonly ISender _sender;
    public GroupsController(ISender sender) => _sender = sender;

    [HttpGet]
    [Authorize(Policy = Roles.Policies.GroupsAccess)]
    public Task<PagedResult<GroupDto>> List(
        [FromQuery] Guid? branchId,
        [FromQuery] Guid? teacherId,
        [FromQuery] bool? isActive,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50,
        CancellationToken ct = default)
        => _sender.Send(new GetGroupsQuery(branchId, teacherId, isActive, page, pageSize), ct);

    [HttpGet("{id:guid}")]
    [Authorize(Policy = Roles.Policies.GroupsAccess)]
    public Task<GroupDetailsDto> GetById(Guid id, CancellationToken ct)
        => _sender.Send(new GetGroupByIdQuery(id), ct);

    [HttpPost]
    [Authorize(Policy = Roles.Policies.CrmAccess)]
    public async Task<IActionResult> Create([FromBody] CreateGroupCommand cmd, CancellationToken ct)
    {
        var id = await _sender.Send(cmd, ct);
        return CreatedAtAction(nameof(GetById), new { id }, new { id });
    }

    [HttpPut("{id:guid}")]
    [Authorize(Policy = Roles.Policies.CrmAccess)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateGroupBody body, CancellationToken ct)
    {
        await _sender.Send(new UpdateGroupCommand(id, body.Name, body.Price, body.Description), ct);
        return NoContent();
    }

    [HttpPatch("{id:guid}/active")]
    [Authorize(Policy = Roles.Policies.CrmAccess)]
    public async Task<IActionResult> ToggleActive(Guid id, [FromBody] ToggleActiveBody body, CancellationToken ct)
    {
        await _sender.Send(new ToggleGroupActiveCommand(id, body.IsActive), ct);
        return NoContent();
    }

    [HttpPatch("{id:guid}/teacher")]
    [Authorize(Policy = Roles.Policies.CrmAccess)]
    public async Task<IActionResult> AssignTeacher(Guid id, [FromBody] AssignTeacherBody body, CancellationToken ct)
    {
        await _sender.Send(new AssignGroupTeacherCommand(id, body.TeacherId), ct);
        return NoContent();
    }

    [HttpPatch("{id:guid}/room")]
    [Authorize(Policy = Roles.Policies.CrmAccess)]
    public async Task<IActionResult> AssignRoom(Guid id, [FromBody] AssignRoomBody body, CancellationToken ct)
    {
        await _sender.Send(new AssignGroupRoomCommand(id, body.RoomId), ct);
        return NoContent();
    }

    [HttpPost("{id:guid}/students")]
    [Authorize(Policy = Roles.Policies.CrmAccess)]
    public async Task<IActionResult> AddStudent(Guid id, [FromBody] AddStudentBody body, CancellationToken ct)
    {
        await _sender.Send(new AddStudentToGroupCommand(id, body.StudentId), ct);
        return NoContent();
    }

    [HttpDelete("{id:guid}/students/{studentId:guid}")]
    [Authorize(Policy = Roles.Policies.CrmAccess)]
    public async Task<IActionResult> RemoveStudent(Guid id, Guid studentId, CancellationToken ct)
    {
        await _sender.Send(new RemoveStudentFromGroupCommand(id, studentId), ct);
        return NoContent();
    }

    [HttpPut("{id:guid}/schedule")]
    [Authorize(Policy = Roles.Policies.CrmAccess)]
    public async Task<IActionResult> SetSchedule(Guid id, [FromBody] List<ScheduleSlotInput> slots, CancellationToken ct)
    {
        await _sender.Send(new SetGroupScheduleCommand(id, slots), ct);
        return NoContent();
    }

    [HttpGet("{id:guid}/attendance")]
    [Authorize(Policy = Roles.Policies.GroupsAccess)]
    public Task<GroupAttendanceDto> GetAttendance(Guid id, [FromQuery] int year, [FromQuery] int month, CancellationToken ct)
        => _sender.Send(new GetGroupAttendanceQuery(id, year, month), ct);

    [HttpPut("{id:guid}/attendance")]
    [Authorize(Policy = Roles.Policies.GroupsAccess)]
    public async Task<IActionResult> SetAttendance(Guid id, [FromBody] SetAttendanceBody body, CancellationToken ct)
    {
        await _sender.Send(new SetAttendanceCommand(id, body.StudentId, body.LessonDate, body.Status), ct);
        return NoContent();
    }

    [HttpPut("{id:guid}/attendance/date")]
    [Authorize(Policy = Roles.Policies.GroupsAccess)]
    public async Task<IActionResult> SetAttendanceForDate(Guid id, [FromBody] SetAttendanceForDateBody body, CancellationToken ct)
    {
        await _sender.Send(new SetAttendanceForDateCommand(id, body.LessonDate, body.Status), ct);
        return NoContent();
    }

    [HttpPatch("{id:guid}/students/{studentId:guid}/status")]
    [Authorize(Policy = Roles.Policies.CrmAccess)]
    public async Task<IActionResult> SetMembershipStatus(Guid id, Guid studentId, [FromBody] SetMembershipStatusBody body, CancellationToken ct)
    {
        await _sender.Send(new SetMembershipStatusCommand(id, studentId, body.Status), ct);
        return NoContent();
    }

    [HttpPut("{id:guid}/students/{studentId:guid}/discount")]
    [Authorize(Policy = Roles.Policies.CrmAccess)]
    public async Task<IActionResult> SetDiscount(Guid id, Guid studentId, [FromBody] SetDiscountBody body, CancellationToken ct)
    {
        await _sender.Send(new SetDiscountCommand(id, studentId, body.Price, body.StartDate, body.EndDate), ct);
        return NoContent();
    }

    [HttpDelete("{id:guid}/students/{studentId:guid}/discount")]
    [Authorize(Policy = Roles.Policies.CrmAccess)]
    public async Task<IActionResult> RemoveDiscount(Guid id, Guid studentId, CancellationToken ct)
    {
        await _sender.Send(new RemoveDiscountCommand(id, studentId), ct);
        return NoContent();
    }

    public record UpdateGroupBody(string Name, decimal Price, string? Description);
    public record ToggleActiveBody(bool IsActive);
    public record AssignTeacherBody(Guid? TeacherId);
    public record AssignRoomBody(Guid? RoomId);
    public record AddStudentBody(Guid StudentId);
    public record SetAttendanceBody(Guid StudentId, DateOnly LessonDate, AttendanceStatus? Status);
    public record SetAttendanceForDateBody(DateOnly LessonDate, AttendanceStatus? Status);
    public record SetMembershipStatusBody(GroupMembershipStatus Status);
    public record SetDiscountBody(decimal Price, DateOnly StartDate, DateOnly EndDate);
}
