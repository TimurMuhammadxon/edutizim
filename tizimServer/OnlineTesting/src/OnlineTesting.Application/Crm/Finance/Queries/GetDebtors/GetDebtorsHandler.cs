using MediatR;
using Microsoft.EntityFrameworkCore;
using OnlineTesting.Application.Common.Interfaces;
using OnlineTesting.Domain.Crm;
using OnlineTesting.Domain.Users;

namespace OnlineTesting.Application.Crm.Finance.Queries.GetDebtors;

public class GetDebtorsHandler : IRequestHandler<GetDebtorsQuery, List<DebtorDto>>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUser _currentUser;

    public GetDebtorsHandler(IApplicationDbContext db, ICurrentUser currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<List<DebtorDto>> Handle(GetDebtorsQuery request, CancellationToken ct)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        var query =
            from gs in _db.GroupStudents.AsNoTracking()
            join g in _db.Groups.AsNoTracking() on gs.GroupId equals g.Id
            join s in _db.Students.AsNoTracking() on gs.StudentId equals s.Id
            // Trial members were never billed (no ActivatedAt), so they can never owe anything — but
            // Frozen/Left members can still genuinely be in debt from before they froze/left, and
            // freezing should only stop new charges, not hide debt already accrued.
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

        var debtors = new List<DebtorDto>();
        foreach (var x in rows)
        {
            var effectivePrice = x.Membership.EffectivePrice(x.Group.Price, today);
            var membershipPayments = paymentsLookup[(x.Group.Id, x.Student.Id)];
            var totalPaid = membershipPayments.Sum(p => p.Amount);
            var monthlyPaid = membershipPayments.GroupBy(p => p.ForMonth).ToDictionary(g => g.Key, g => g.Sum(p => p.Amount));
            var asOfDate = x.Membership.BalanceAsOfDate(today);
            var (balance, nextDueDate) = BalanceCalculator.Compute(
                x.Membership.ActivatedAt, totalPaid,
                month => x.Membership.EffectivePriceForMonth(month, x.Group.Price),
                month => monthlyPaid.GetValueOrDefault(month, 0m), asOfDate, today);

            if (balance >= 0 || nextDueDate is null)
                continue;

            var daysOverdue = Math.Max(0, today.DayNumber - nextDueDate.Value.DayNumber);
            debtors.Add(new DebtorDto(
                x.Student.Id, x.Student.FullName, x.Student.Phone,
                x.Group.Id, x.Group.Name, effectivePrice, balance, nextDueDate.Value, daysOverdue));
        }

        return debtors.OrderBy(d => d.Balance).ToList();
    }
}
