using MediatR;
using Microsoft.EntityFrameworkCore;
using OnlineTesting.Application.Common.Exceptions;
using OnlineTesting.Application.Common.Interfaces;
using OnlineTesting.Domain.Crm;

namespace OnlineTesting.Application.Crm.Groups.Commands.AddStudentToGroup;

public class AddStudentToGroupHandler : IRequestHandler<AddStudentToGroupCommand>
{
    private readonly IApplicationDbContext _db;

    public AddStudentToGroupHandler(IApplicationDbContext db) => _db = db;

    public async Task Handle(AddStudentToGroupCommand request, CancellationToken ct)
    {
        var group = await _db.Groups.FirstOrDefaultAsync(g => g.Id == request.GroupId, ct)
            ?? throw new NotFoundException($"Group '{request.GroupId}' not found.");

        var studentExists = await _db.Students.AnyAsync(s => s.Id == request.StudentId, ct);
        if (!studentExists)
            throw new NotFoundException($"Student '{request.StudentId}' not found.");

        var alreadyMember = await _db.GroupStudents.AnyAsync(
            gs => gs.GroupId == request.GroupId && gs.StudentId == request.StudentId, ct);
        if (alreadyMember)
            throw new ConflictException("Student is already in this group.");

        if (group.RoomId.HasValue)
        {
            var room = await _db.Rooms.AsNoTracking().FirstOrDefaultAsync(r => r.Id == group.RoomId.Value, ct);
            if (room is not null)
            {
                var currentCount = await _db.GroupStudents.CountAsync(gs => gs.GroupId == request.GroupId, ct);
                if (currentCount >= room.Capacity)
                    throw new ConflictException($"Room '{room.Name}' capacity ({room.Capacity}) reached.");
            }
        }

        _db.GroupStudents.Add(GroupStudent.Create(request.GroupId, request.StudentId));
        await _db.SaveChangesAsync(ct);
    }
}
