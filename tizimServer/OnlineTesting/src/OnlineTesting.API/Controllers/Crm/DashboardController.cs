using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnlineTesting.Application.Crm.Dashboard.Queries.GetDashboardSummary;
using OnlineTesting.Domain.Authorization;

namespace OnlineTesting.API.Controllers.Crm;

[ApiController]
[Route("crm/dashboard")]
[Authorize(Policy = Roles.Policies.CrmAccess)]
public class DashboardController : ControllerBase
{
    private readonly ISender _sender;
    public DashboardController(ISender sender) => _sender = sender;

    [HttpGet]
    public Task<DashboardSummaryDto> GetSummary([FromQuery] Guid? branchId, CancellationToken ct)
        => _sender.Send(new GetDashboardSummaryQuery(branchId), ct);
}
