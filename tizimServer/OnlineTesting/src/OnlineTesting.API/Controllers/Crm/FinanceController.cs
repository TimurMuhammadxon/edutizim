using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnlineTesting.Application.Common.Models;
using OnlineTesting.Application.Crm.Finance.Commands.DeletePayment;
using OnlineTesting.Application.Crm.Finance.Commands.RecordPayment;
using OnlineTesting.Application.Crm.Finance.Queries.GetDebtors;
using OnlineTesting.Application.Crm.Finance.Queries.GetPayments;
using OnlineTesting.Application.Crm.Finance.Queries.GetPaymentsSummary;
using OnlineTesting.Application.Crm.Finance.Queries.GetPeriodDebts;
using OnlineTesting.Domain.Authorization;

namespace OnlineTesting.API.Controllers.Crm;

[ApiController]
[Route("crm/finance")]
public class FinanceController : ControllerBase
{
    private readonly ISender _sender;
    public FinanceController(ISender sender) => _sender = sender;

    [HttpGet("payments")]
    [Authorize(Policy = Roles.Policies.GroupsAccess)]
    public Task<PagedResult<PaymentDto>> GetPayments(
        [FromQuery] Guid? groupId,
        [FromQuery] Guid? studentId,
        [FromQuery] Guid? branchId,
        [FromQuery] DateOnly? fromDate,
        [FromQuery] DateOnly? toDate,
        [FromQuery] string? search,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50,
        CancellationToken ct = default)
        => _sender.Send(new GetPaymentsQuery(groupId, studentId, branchId, fromDate, toDate, search, page, pageSize), ct);

    [HttpGet("payments/summary")]
    [Authorize(Policy = Roles.Policies.GroupsAccess)]
    public Task<PaymentsSummaryDto> GetPaymentsSummary(
        [FromQuery] Guid? groupId,
        [FromQuery] Guid? studentId,
        [FromQuery] Guid? branchId,
        [FromQuery] DateOnly? fromDate,
        [FromQuery] DateOnly? toDate,
        [FromQuery] string? search,
        CancellationToken ct = default)
        => _sender.Send(new GetPaymentsSummaryQuery(groupId, studentId, branchId, fromDate, toDate, search), ct);

    [HttpPost("payments")]
    [Authorize(Policy = Roles.Policies.CrmAccess)]
    public async Task<IActionResult> RecordPayment([FromBody] RecordPaymentCommand cmd, CancellationToken ct)
    {
        var id = await _sender.Send(cmd, ct);
        return Ok(new { id });
    }

    [HttpDelete("payments/{id:guid}")]
    [Authorize(Policy = Roles.Policies.CrmAccess)]
    public async Task<IActionResult> DeletePayment(Guid id, CancellationToken ct)
    {
        await _sender.Send(new DeletePaymentCommand(id), ct);
        return NoContent();
    }

    [HttpGet("debtors")]
    [Authorize(Policy = Roles.Policies.GroupsAccess)]
    public Task<PagedResult<DebtorDto>> GetDebtors(
        [FromQuery] Guid? branchId,
        [FromQuery] Guid? groupId,
        [FromQuery] string? search,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50,
        CancellationToken ct = default)
        => _sender.Send(new GetDebtorsQuery(branchId, groupId, search, page, pageSize), ct);

    [HttpGet("period-debts")]
    [Authorize(Policy = Roles.Policies.GroupsAccess)]
    public Task<PagedResult<PeriodDebtDto>> GetPeriodDebts(
        [FromQuery] DateOnly fromDate,
        [FromQuery] DateOnly toDate,
        [FromQuery] Guid? branchId,
        [FromQuery] Guid? groupId,
        [FromQuery] string? search,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50,
        CancellationToken ct = default)
        => _sender.Send(new GetPeriodDebtsQuery(fromDate, toDate, branchId, groupId, search, page, pageSize), ct);
}
