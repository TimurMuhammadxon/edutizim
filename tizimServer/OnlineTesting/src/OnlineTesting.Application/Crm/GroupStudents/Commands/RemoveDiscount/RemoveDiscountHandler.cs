using MediatR;
using Microsoft.EntityFrameworkCore;
using OnlineTesting.Application.Common.Exceptions;
using OnlineTesting.Application.Common.Interfaces;

namespace OnlineTesting.Application.Crm.GroupStudents.Commands.RemoveDiscount;

public class RemoveDiscountHandler : IRequestHandler<RemoveDiscountCommand>
{
    private readonly IApplicationDbContext _db;

    public RemoveDiscountHandler(IApplicationDbContext db) => _db = db;

    public async Task Handle(RemoveDiscountCommand request, CancellationToken ct)
    {
        var groupExists = await _db.Groups.AnyAsync(g => g.Id == request.GroupId, ct);
        if (!groupExists)
            throw new NotFoundException($"Group '{request.GroupId}' not found.");

        var membership = await _db.GroupStudents.FirstOrDefaultAsync(
            gs => gs.GroupId == request.GroupId && gs.StudentId == request.StudentId, ct)
            ?? throw new NotFoundException($"Student '{request.StudentId}' is not a member of this group.");

        membership.RemoveDiscount();
        await _db.SaveChangesAsync(ct);
    }
}
