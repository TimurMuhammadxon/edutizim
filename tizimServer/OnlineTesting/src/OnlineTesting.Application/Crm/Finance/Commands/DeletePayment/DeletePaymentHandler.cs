using MediatR;
using Microsoft.EntityFrameworkCore;
using OnlineTesting.Application.Common.Exceptions;
using OnlineTesting.Application.Common.Interfaces;

namespace OnlineTesting.Application.Crm.Finance.Commands.DeletePayment;

public class DeletePaymentHandler : IRequestHandler<DeletePaymentCommand>
{
    private readonly IApplicationDbContext _db;

    public DeletePaymentHandler(IApplicationDbContext db) => _db = db;

    public async Task Handle(DeletePaymentCommand request, CancellationToken ct)
    {
        var payment = await _db.TuitionPayments.FirstOrDefaultAsync(p => p.Id == request.Id, ct)
            ?? throw new NotFoundException($"Payment '{request.Id}' not found.");

        _db.TuitionPayments.Remove(payment);
        await _db.SaveChangesAsync(ct);
    }
}
