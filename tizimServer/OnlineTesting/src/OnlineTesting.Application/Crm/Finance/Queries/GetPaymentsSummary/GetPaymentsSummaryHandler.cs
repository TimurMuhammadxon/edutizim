using MediatR;
using Microsoft.EntityFrameworkCore;
using OnlineTesting.Application.Common.Interfaces;
using OnlineTesting.Domain.Users;

namespace OnlineTesting.Application.Crm.Finance.Queries.GetPaymentsSummary;

public class GetPaymentsSummaryHandler : IRequestHandler<GetPaymentsSummaryQuery, PaymentsSummaryDto>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUser _currentUser;

    public GetPaymentsSummaryHandler(IApplicationDbContext db, ICurrentUser currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<PaymentsSummaryDto> Handle(GetPaymentsSummaryQuery request, CancellationToken ct)
    {
        var query =
            from p in _db.TuitionPayments.AsNoTracking()
            join g in _db.Groups.AsNoTracking() on p.GroupId equals g.Id
            join s in _db.Students.AsNoTracking() on p.StudentId equals s.Id
            select new { Payment = p, GroupTeacherId = g.TeacherId, GroupBranchId = g.BranchId, StudentName = s.FullName, StudentPhone = s.Phone };

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

        var count = await query.CountAsync(ct);
        var total = count == 0 ? 0m : await query.SumAsync(x => x.Payment.Amount, ct);

        return new PaymentsSummaryDto(total, count);
    }
}
