using MediatR;
using Microsoft.EntityFrameworkCore;
using OnlineTesting.Application.Common.Interfaces;
using OnlineTesting.Application.Common.Models;
using OnlineTesting.Domain.Crm;
using OnlineTesting.Domain.Users;

namespace OnlineTesting.Application.Crm.Finance.Queries.GetPeriodDebts;

/// Which specific months (within the requested date range) a membership still owes money for —
/// as opposed to GetDebtorsQuery, which reports only the current, as-of-today aggregate balance.
public class GetPeriodDebtsHandler : IRequestHandler<GetPeriodDebtsQuery, PagedResult<PeriodDebtDto>>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUser _currentUser;

    public GetPeriodDebtsHandler(IApplicationDbContext db, ICurrentUser currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<PagedResult<PeriodDebtDto>> Handle(GetPeriodDebtsQuery request, CancellationToken ct)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        var query =
            from gs in _db.GroupStudents.AsNoTracking()
            join g in _db.Groups.AsNoTracking() on gs.GroupId equals g.Id
            join s in _db.Students.AsNoTracking() on gs.StudentId equals s.Id
            where gs.Status != GroupMembershipStatus.Trial
            select new { Membership = gs, Group = g, Student = s };

        if (_currentUser.Role == Role.Teacher)
            query = query.Where(x => x.Group.TeacherId == _currentUser.UserId);

        if (request.BranchId.HasValue)
            query = query.Where(x => x.Group.BranchId == request.BranchId.Value);

        if (request.GroupId.HasValue)
            query = query.Where(x => x.Group.Id == request.GroupId.Value);

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var term = request.Search.Trim().ToLower();
            query = query.Where(x => x.Student.FullName.ToLower().Contains(term) || x.Student.Phone.ToLower().Contains(term));
        }

        var rows = await query.ToListAsync(ct);

        var groupIds = rows.Select(x => x.Group.Id).Distinct().ToList();
        var payments = await _db.TuitionPayments.AsNoTracking()
            .Where(p => groupIds.Contains(p.GroupId))
            .Select(p => new { p.GroupId, p.StudentId, p.ForMonth, p.Amount })
            .ToListAsync(ct);
        var paymentsLookup = payments.ToLookup(p => (p.GroupId, p.StudentId));

        var periodStartMonth = new DateOnly(request.FromDate.Year, request.FromDate.Month, 1);
        var periodEndMonth = new DateOnly(request.ToDate.Year, request.ToDate.Month, 1);
        // A month only counts as owed once it has fully elapsed — the current calendar month is
        // never "due" yet, no matter how many days into it we are (mirrors BalanceCalculator.Compute).
        var lastElapsedMonth = new DateOnly(today.Year, today.Month, 1).AddMonths(-1);

        var result = new List<PeriodDebtDto>();
        foreach (var x in rows)
        {
            if (x.Membership.ActivatedAt is null)
                continue; // never activated (shouldn't happen outside Trial, kept defensively)

            var activatedMonth = new DateOnly(x.Membership.ActivatedAt.Value.Year, x.Membership.ActivatedAt.Value.Month, 1);
            var asOfDate = x.Membership.BalanceAsOfDate(today); // caps at freeze date while frozen
            var asOfMonth = new DateOnly(asOfDate.Year, asOfDate.Month, 1);
            var chargeableMonth = asOfMonth < lastElapsedMonth ? asOfMonth : lastElapsedMonth;

            var monthFrom = periodStartMonth > activatedMonth ? periodStartMonth : activatedMonth;
            var monthTo = periodEndMonth < chargeableMonth ? periodEndMonth : chargeableMonth;
            if (monthFrom > monthTo)
                continue;

            var membershipPayments = paymentsLookup[(x.Group.Id, x.Student.Id)];
            var monthlyPaid = membershipPayments.GroupBy(p => p.ForMonth).ToDictionary(g => g.Key, g => g.Sum(p => p.Amount));

            var months = new List<PeriodDebtMonthDto>();
            for (var month = monthFrom; month <= monthTo; month = month.AddMonths(1))
            {
                var expected = x.Membership.EffectivePriceForMonth(month, x.Group.Price);
                var paid = monthlyPaid.GetValueOrDefault(month, 0m);
                if (paid < expected)
                    months.Add(new PeriodDebtMonthDto(month, expected, paid, expected - paid));
            }

            if (months.Count == 0)
                continue;

            result.Add(new PeriodDebtDto(
                x.Student.Id, x.Student.FullName, x.Student.Phone,
                x.Group.Id, x.Group.Name,
                months.Sum(m => m.Shortfall),
                months));
        }

        var ordered = result.OrderByDescending(d => d.AmountOwedInPeriod).ToList();

        var page = Math.Max(1, request.Page);
        var pageSize = Math.Clamp(request.PageSize, 1, 100);
        var pageItems = ordered.Skip((page - 1) * pageSize).Take(pageSize).ToList();

        return new PagedResult<PeriodDebtDto>(pageItems, page, pageSize, ordered.Count);
    }
}
