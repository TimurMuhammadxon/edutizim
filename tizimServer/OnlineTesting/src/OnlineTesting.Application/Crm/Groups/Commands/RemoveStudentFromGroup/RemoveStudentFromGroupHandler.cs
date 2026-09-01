using MediatR;
using Microsoft.EntityFrameworkCore;
using OnlineTesting.Application.Common.Exceptions;
using OnlineTesting.Application.Common.Interfaces;

namespace OnlineTesting.Application.Crm.Groups.Commands.RemoveStudentFromGroup;

public class RemoveStudentFromGroupHandler : IRequestHandler<RemoveStudentFromGroupCommand>
{
    private readonly IApplicationDbContext _db;

    public RemoveStudentFromGroupHandler(IApplicationDbContext db) => _db = db;

    public async Task Handle(RemoveStudentFromGroupCommand request, CancellationToken ct)
    {
        var membership = await _db.GroupStudents.FirstOrDefaultAsync(
            gs => gs.GroupId == request.GroupId && gs.StudentId == request.StudentId, ct)
            ?? throw new NotFoundException("Student is not a member of this group.");

        _db.GroupStudents.Remove(membership);
        await _db.SaveChangesAsync(ct);
    }
}
