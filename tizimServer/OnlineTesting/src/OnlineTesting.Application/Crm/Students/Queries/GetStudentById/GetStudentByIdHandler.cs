using MediatR;
using Microsoft.EntityFrameworkCore;
using OnlineTesting.Application.Common.Exceptions;
using OnlineTesting.Application.Common.Interfaces;
using OnlineTesting.Domain.Crm;

namespace OnlineTesting.Application.Crm.Students.Queries.GetStudentById;

public class GetStudentByIdHandler : IRequestHandler<GetStudentByIdQuery, StudentDetailsDto>
{
    private readonly IApplicationDbContext _db;

    public GetStudentByIdHandler(IApplicationDbContext db) => _db = db;

    public async Task<StudentDetailsDto> Handle(GetStudentByIdQuery request, CancellationToken ct)
    {
        var row = await (
            from s in _db.Students.AsNoTracking()
            join l in _db.Leads.AsNoTracking() on s.LeadId equals (Guid?)l.Id into leadJoin
            from l in leadJoin.DefaultIfEmpty()
            join b in _db.Branches.AsNoTracking() on s.BranchId equals b.Id
            where s.Id == request.Id
            select new { Student = s, Lead = l, BranchName = b.Name }
        ).FirstOrDefaultAsync(ct)
            ?? throw new NotFoundException($"Student '{request.Id}' not found.");

        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        var membershipRows = await (
            from gs in _db.GroupStudents.AsNoTracking()
            join g in _db.Groups.AsNoTracking() on gs.GroupId equals g.Id
            join t in _db.Users.AsNoTracking() on g.TeacherId equals (Guid?)t.Id into teacherJoin
            from t in teacherJoin.DefaultIfEmpty()
            where gs.StudentId == request.Id
            select new
            {
                Membership = gs,
                Group = g,
                TeacherName = t == null ? null : ((t.FirstName ?? "") + (t.LastName != null ? " " + t.LastName : ""))
            }
        ).ToListAsync(ct);

        var groupIds = membershipRows.Select(x => x.Group.Id).ToList();

        var payments = await _db.TuitionPayments.AsNoTracking()
            .Where(p => p.StudentId == request.Id && groupIds.Contains(p.GroupId))
            .Select(p => new { p.GroupId, p.ForMonth, p.Amount })
            .ToListAsync(ct);
        var paymentsByGroup = payments.ToLookup(p => p.GroupId);

        var attendanceCounts = await _db.Attendances.AsNoTracking()
            .Where(a => a.StudentId == request.Id && groupIds.Contains(a.GroupId))
            .GroupBy(a => new { a.GroupId, a.Status })
            .Select(g => new { g.Key.GroupId, g.Key.Status, Count = g.Count() })
            .ToListAsync(ct);

        var groups = membershipRows.Select(x =>
        {
            var effectivePrice = x.Membership.EffectivePrice(x.Group.Price, today);
            var groupPayments = paymentsByGroup[x.Group.Id];
            var totalPaid = groupPayments.Sum(p => p.Amount);
            var monthlyPaid = groupPayments.GroupBy(p => p.ForMonth).ToDictionary(g => g.Key, g => g.Sum(p => p.Amount));
            var asOfDate = x.Membership.BalanceAsOfDate(today);
            var (balance, nextDueDate) = BalanceCalculator.Compute(
                x.Membership.ActivatedAt, totalPaid,
                month => x.Membership.EffectivePriceForMonth(month, x.Group.Price),
                month => monthlyPaid.GetValueOrDefault(month, 0m), asOfDate, today);

            var presentCount = attendanceCounts.FirstOrDefault(a => a.GroupId == x.Group.Id && a.Status == AttendanceStatus.Present)?.Count ?? 0;
            var absentCount = attendanceCounts.FirstOrDefault(a => a.GroupId == x.Group.Id && a.Status == AttendanceStatus.Absent)?.Count ?? 0;

            return new StudentGroupMembershipDto(
                x.Group.Id, x.Group.Name, x.TeacherName, x.Membership.Status, x.Membership.JoinedAt,
                x.Membership.ActivatedAt, effectivePrice, balance, nextDueDate, presentCount, absentCount);
        }).ToList();

        var totalBalance = groups.Sum(g => g.Balance);
        var startedAt = membershipRows
            .Where(x => x.Membership.ActivatedAt.HasValue)
            .Select(x => x.Membership.ActivatedAt!.Value)
            .OrderBy(d => d)
            .Cast<DateTime?>()
            .FirstOrDefault();

        return new StudentDetailsDto(
            row.Student.Id, row.Student.BranchId, row.BranchName, row.Student.LeadId, row.Lead?.FullName, row.Student.UserId,
            row.Student.FullName, row.Student.Phone, row.Student.Email,
            row.Student.IsActive, row.Student.Notes, row.Student.CreatedAt, startedAt,
            totalBalance, groups);
    }
}
