using MediatR;
using Microsoft.EntityFrameworkCore;
using OnlineTesting.Application.Common.Interfaces;
using OnlineTesting.Application.Common.Models;
using OnlineTesting.Domain.Users;

namespace OnlineTesting.Application.Crm.Finance.Queries.GetPayments;

public class GetPaymentsHandler : IRequestHandler<GetPaymentsQuery, PagedResult<PaymentDto>>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUser _currentUser;

    public GetPaymentsHandler(IApplicationDbContext db, ICurrentUser currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<PagedResult<PaymentDto>> Handle(GetPaymentsQuery request, CancellationToken ct)
    {
        var page = Math.Max(1, request.Page);
        var size = Math.Clamp(request.PageSize, 1, 200);

        var query =
            from p in _db.TuitionPayments.AsNoTracking()
            join g in _db.Groups.AsNoTracking() on p.GroupId equals g.Id
            join s in _db.Students.AsNoTracking() on p.StudentId equals s.Id
            select new { Payment = p, GroupName = g.Name, GroupTeacherId = g.TeacherId, GroupBranchId = g.BranchId, StudentName = s.FullName, StudentPhone = s.Phone };

        if (_currentUser.Role == Role.Teacher)
            query = query.Where(x => x.GroupTeacherId == _currentUser.UserId);

        if (request.GroupId.HasValue)
            query = query.Where(x => x.Payment.GroupId == request.GroupId.Value);

        if (request.StudentId.HasValue)
            query = query.Where(x => x.Payment.StudentId == request.StudentId.Value);

        if (request.BranchId.HasValue)
            query = query.Where(x => x.GroupBranchId == request.BranchId.Value);

        if (request.FromDate.HasValue)
            query = query.Where(x => x.Payment.PaidAt >= request.FromDate.Value);

        if (request.ToDate.HasValue)
            query = query.Where(x => x.Payment.PaidAt <= request.ToDate.Value);

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var term = request.Search.Trim().ToLower();
            query = query.Where(x => x.StudentName.ToLower().Contains(term) || x.StudentPhone.ToLower().Contains(term));
        }

        var total = await query.CountAsync(ct);

        var rows = await query
            .OrderByDescending(x => x.Payment.PaidAt).ThenByDescending(x => x.Payment.CreatedAt)
            .Skip((page - 1) * size)
            .Take(size)
            .ToListAsync(ct);

        var items = rows.Select(x => new PaymentDto(
            x.Payment.Id, x.Payment.GroupId, x.GroupName, x.Payment.StudentId, x.StudentName,
            x.Payment.Amount, x.Payment.PaidAt, x.Payment.ForMonth, x.Payment.Method, x.Payment.Note, x.Payment.CreatedAt))
            .ToList();

        return new PagedResult<PaymentDto>(items, page, size, total);
    }
}
