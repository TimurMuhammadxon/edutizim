using MediatR;
using Microsoft.EntityFrameworkCore;
using OnlineTesting.Application.Common.Interfaces;
using OnlineTesting.Application.Common.Models;
using OnlineTesting.Domain.Crm;

namespace OnlineTesting.Application.Crm.Students.Queries.GetStudents;

public class GetStudentsHandler : IRequestHandler<GetStudentsQuery, PagedResult<StudentDto>>
{
    private readonly IApplicationDbContext _db;

    public GetStudentsHandler(IApplicationDbContext db) => _db = db;

    public async Task<PagedResult<StudentDto>> Handle(GetStudentsQuery request, CancellationToken ct)
    {
        var page = Math.Max(1, request.Page);
        var size = Math.Clamp(request.PageSize, 1, 100);

        var nowUtc = DateTime.UtcNow;
        var today = DateOnly.FromDateTime(nowUtc);
        var monthStartUtc = new DateTime(nowUtc.Year, nowUtc.Month, 1, 0, 0, 0, DateTimeKind.Utc);
        var nextMonthUtc = monthStartUtc.AddMonths(1);
        var monthStart = DateOnly.FromDateTime(monthStartUtc);
        var nextMonth = DateOnly.FromDateTime(nextMonthUtc);

        var query = _db.Students.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var term = request.Search.Trim().ToLower();
            query = query.Where(s => s.FullName.ToLower().Contains(term) || s.Phone.ToLower().Contains(term));
        }

        if (request.BranchId.HasValue)
        {
            var bid = request.BranchId.Value;
            // A student "belongs" to a branch either as their home/registration branch, or by
            // actually studying in a group that belongs to it (cross-branch enrollment is allowed).
            var studentIdsWithGroupInBranch = _db.GroupStudents.AsNoTracking()
                .Join(_db.Groups.AsNoTracking(), gs => gs.GroupId, g => g.Id, (gs, g) => new { gs.StudentId, g.BranchId })
                .Where(x => x.BranchId == bid)
                .Select(x => x.StudentId);

            query = query.Where(s => s.BranchId == bid || studentIdsWithGroupInBranch.Contains(s.Id));
        }

        if (request.GroupId.HasValue)
        {
            var gid = request.GroupId.Value;
            query = query.Where(s => _db.GroupStudents.Any(gs => gs.StudentId == s.Id && gs.GroupId == gid));
        }

        if (request.IsActive.HasValue)
            query = query.Where(s => s.IsActive == request.IsActive.Value);

        query = request.StudentStatus switch
        {
            "AddedThisMonth" => query.Where(s => s.CreatedAt >= monthStartUtc && s.CreatedAt < nextMonthUtc),
            "Trial" => query.Where(s => _db.GroupStudents.Any(gs => gs.StudentId == s.Id && gs.Status == GroupMembershipStatus.Trial)),
            "Active" => query.Where(s => _db.GroupStudents.Any(gs => gs.StudentId == s.Id && gs.Status == GroupMembershipStatus.Active)),
            "Frozen" => query.Where(s => _db.GroupStudents.Any(gs => gs.StudentId == s.Id && gs.Status == GroupMembershipStatus.Frozen)),
            "WithoutGroup" => query.Where(s => !_db.GroupStudents.Any(gs => gs.StudentId == s.Id)),
            "LeftAfterTrial" => query.Where(s => _db.GroupStudents.Any(gs =>
                gs.StudentId == s.Id && gs.Status == GroupMembershipStatus.Left && gs.ActivatedAt == null)),
            _ => query,
        };

        if (string.IsNullOrWhiteSpace(request.FinancialStatus))
        {
            var total = await query.CountAsync(ct);

            var items = await query
                .OrderByDescending(s => s.CreatedAt)
                .Skip((page - 1) * size)
                .Take(size)
                .Select(s => new StudentDto(
                    s.Id, s.BranchId, s.LeadId, s.UserId, s.FullName, s.Phone, s.Email, s.IsActive, s.Notes, s.CreatedAt))
                .ToListAsync(ct);

            return new PagedResult<StudentDto>(items, page, size, total);
        }

        // Financial filters need per-membership Balance (via BalanceCalculator), which can't be
        // expressed in SQL — materialize the candidates first, then filter/paginate in memory.
        var candidates = await query
            .OrderByDescending(s => s.CreatedAt)
            .Select(s => new StudentDto(
                s.Id, s.BranchId, s.LeadId, s.UserId, s.FullName, s.Phone, s.Email, s.IsActive, s.Notes, s.CreatedAt))
            .ToListAsync(ct);
        var candidateIds = candidates.Select(c => c.Id).ToList();

        var memberships = await _db.GroupStudents.AsNoTracking()
            .Where(gs => candidateIds.Contains(gs.StudentId))
            .Join(_db.Groups.AsNoTracking(), gs => gs.GroupId, g => g.Id, (gs, g) => new { Membership = gs, g.Price })
            .ToListAsync(ct);

        var payments = await _db.TuitionPayments.AsNoTracking()
            .Where(p => candidateIds.Contains(p.StudentId))
            .Select(p => new { p.StudentId, p.GroupId, p.ForMonth, p.Amount, p.PaidAt })
            .ToListAsync(ct);
        var paymentsLookup = payments.ToLookup(p => (p.StudentId, p.GroupId));

        bool Matches(Guid studentId)
        {
            if (request.FinancialStatus == "PaidThisMonth")
                return payments.Any(p => p.StudentId == studentId && p.PaidAt >= monthStart && p.PaidAt < nextMonth);

            var studentMemberships = memberships.Where(m => m.Membership.StudentId == studentId).ToList();

            if (request.FinancialStatus == "WithDiscount")
                return studentMemberships.Any(m =>
                    m.Membership.DiscountedPrice.HasValue && m.Membership.DiscountStartDate.HasValue && m.Membership.DiscountEndDate.HasValue
                    && m.Membership.DiscountStartDate.Value <= today && m.Membership.DiscountEndDate.Value >= today);

            var balances = studentMemberships
                .Where(m => m.Membership.ActivatedAt != null)
                .Select(m =>
                {
                    var membershipPayments = paymentsLookup[(studentId, m.Membership.GroupId)];
                    var totalPaid = membershipPayments.Sum(p => p.Amount);
                    var monthlyPaid = membershipPayments.GroupBy(p => p.ForMonth).ToDictionary(g => g.Key, g => g.Sum(p => p.Amount));
                    var asOfDate = m.Membership.BalanceAsOfDate(today);
                    var (balance, _) = BalanceCalculator.Compute(
                        m.Membership.ActivatedAt, totalPaid,
                        month => m.Membership.EffectivePriceForMonth(month, m.Price),
                        month => monthlyPaid.GetValueOrDefault(month, 0m), asOfDate, today);
                    return balance;
                })
                .ToList();

            return request.FinancialStatus switch
            {
                "WithDebt" => balances.Any(b => b < 0),
                "WithoutDebt" => !balances.Any(b => b < 0),
                "PositiveBalance" => balances.Any(b => b > 0),
                _ => true,
            };
        }

        var filtered = candidates.Where(c => Matches(c.Id)).ToList();
        var pagedItems = filtered.Skip((page - 1) * size).Take(size).ToList();

        return new PagedResult<StudentDto>(pagedItems, page, size, filtered.Count);
    }
}
