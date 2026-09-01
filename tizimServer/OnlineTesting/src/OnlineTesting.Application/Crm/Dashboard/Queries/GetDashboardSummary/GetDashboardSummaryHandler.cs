using MediatR;
using Microsoft.EntityFrameworkCore;
using OnlineTesting.Application.Common.Interfaces;
using OnlineTesting.Domain.Crm;

namespace OnlineTesting.Application.Crm.Dashboard.Queries.GetDashboardSummary;

public class GetDashboardSummaryHandler : IRequestHandler<GetDashboardSummaryQuery, DashboardSummaryDto>
{
    private readonly IApplicationDbContext _db;

    public GetDashboardSummaryHandler(IApplicationDbContext db) => _db = db;

    public async Task<DashboardSummaryDto> Handle(GetDashboardSummaryQuery request, CancellationToken ct)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var monthStartDate = new DateOnly(today.Year, today.Month, 1);
        var branchId = request.BranchId;

        var leadsQuery = _db.Leads.AsNoTracking()
            .Where(l => l.Stage != LeadStage.Converted && l.Stage != LeadStage.Lost);
        var studentsQuery = _db.Students.AsNoTracking().Where(s => s.IsActive);
        var groupsQuery = _db.Groups.AsNoTracking().Where(g => g.IsActive);

        var membershipQuery =
            from gs in _db.GroupStudents.AsNoTracking()
            join g in _db.Groups.AsNoTracking() on gs.GroupId equals g.Id
            select new { Membership = gs, Group = g };

        if (branchId.HasValue)
        {
            leadsQuery = leadsQuery.Where(l => l.BranchId == branchId.Value);
            studentsQuery = studentsQuery.Where(s => s.BranchId == branchId.Value);
            groupsQuery = groupsQuery.Where(g => g.BranchId == branchId.Value);
            membershipQuery = membershipQuery.Where(x => x.Group.BranchId == branchId.Value);
        }

        var activeLeads = await leadsQuery.CountAsync(ct);
        var activeStudents = await studentsQuery.CountAsync(ct);
        var activeGroups = await groupsQuery.CountAsync(ct);
        var inTrial = await membershipQuery.CountAsync(x => x.Membership.Status == GroupMembershipStatus.Trial, ct);

        var paymentsThisMonthQuery = _db.TuitionPayments.AsNoTracking().Where(p => p.PaidAt >= monthStartDate);
        if (branchId.HasValue)
        {
            var branchGroupIds = await groupsQuery.Select(g => g.Id).ToListAsync(ct);
            paymentsThisMonthQuery = paymentsThisMonthQuery.Where(p => branchGroupIds.Contains(p.GroupId));
        }

        var paidThisMonthCount = await paymentsThisMonthQuery.CountAsync(ct);
        var paidThisMonthAmount = await paymentsThisMonthQuery.SumAsync(p => (decimal?)p.Amount, ct) ?? 0m;

        // Trial members were never billed; Frozen/Left members can still genuinely owe money from
        // before they froze/left, so they're included here (freezing only stops new charges).
        var billableMemberships = await membershipQuery
            .Where(x => x.Membership.Status != GroupMembershipStatus.Trial)
            .ToListAsync(ct);

        var groupIdsForPayments = billableMemberships.Select(x => x.Group.Id).Distinct().ToList();
        var payments = await _db.TuitionPayments.AsNoTracking()
            .Where(p => groupIdsForPayments.Contains(p.GroupId))
            .Select(p => new { p.GroupId, p.StudentId, p.ForMonth, p.Amount })
            .ToListAsync(ct);
        var paymentsLookup = payments.ToLookup(p => (p.GroupId, p.StudentId));

        var debtorStudentIds = new HashSet<Guid>();
        foreach (var x in billableMemberships)
        {
            var membershipPayments = paymentsLookup[(x.Group.Id, x.Membership.StudentId)];
            var totalPaid = membershipPayments.Sum(p => p.Amount);
            var monthlyPaid = membershipPayments.GroupBy(p => p.ForMonth).ToDictionary(g => g.Key, g => g.Sum(p => p.Amount));
            var asOfDate = x.Membership.BalanceAsOfDate(today);
            var (balance, _) = BalanceCalculator.Compute(
                x.Membership.ActivatedAt, totalPaid,
                month => x.Membership.EffectivePriceForMonth(month, x.Group.Price),
                month => monthlyPaid.GetValueOrDefault(month, 0m), asOfDate, today);
            if (balance < 0)
                debtorStudentIds.Add(x.Membership.StudentId);
        }

        return new DashboardSummaryDto(
            activeLeads, activeStudents, activeGroups, debtorStudentIds.Count,
            inTrial, paidThisMonthCount, paidThisMonthAmount);
    }
}
