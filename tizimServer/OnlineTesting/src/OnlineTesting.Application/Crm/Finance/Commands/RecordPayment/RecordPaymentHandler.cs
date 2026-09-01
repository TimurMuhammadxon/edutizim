using MediatR;
using Microsoft.EntityFrameworkCore;
using OnlineTesting.Application.Common.Exceptions;
using OnlineTesting.Application.Common.Interfaces;
using OnlineTesting.Domain.Crm;

namespace OnlineTesting.Application.Crm.Finance.Commands.RecordPayment;

public class RecordPaymentHandler : IRequestHandler<RecordPaymentCommand, Guid>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUser _currentUser;

    public RecordPaymentHandler(IApplicationDbContext db, ICurrentUser currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<Guid> Handle(RecordPaymentCommand request, CancellationToken ct)
    {
        var group = await _db.Groups.FirstOrDefaultAsync(g => g.Id == request.GroupId, ct)
            ?? throw new NotFoundException($"Group '{request.GroupId}' not found.");

        var membership = await _db.GroupStudents.FirstOrDefaultAsync(
            gs => gs.GroupId == request.GroupId && gs.StudentId == request.StudentId, ct)
            ?? throw new NotFoundException($"Student '{request.StudentId}' is not a member of this group.");

        var payment = Payment.Create(
            group.OrganizationId, request.GroupId, request.StudentId, request.Amount,
            request.PaidAt, request.ForMonth, request.Method, _currentUser.UserId!.Value, request.Note);

        _db.TuitionPayments.Add(payment);
        membership.RecordPayment();

        await _db.SaveChangesAsync(ct);
        return payment.Id;
    }
}
