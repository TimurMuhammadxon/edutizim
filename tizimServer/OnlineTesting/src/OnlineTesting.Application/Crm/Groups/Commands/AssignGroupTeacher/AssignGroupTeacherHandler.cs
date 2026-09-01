using MediatR;
using Microsoft.EntityFrameworkCore;
using OnlineTesting.Application.Common.Exceptions;
using OnlineTesting.Application.Common.Interfaces;
using OnlineTesting.Domain.Users;

namespace OnlineTesting.Application.Crm.Groups.Commands.AssignGroupTeacher;

public class AssignGroupTeacherHandler : IRequestHandler<AssignGroupTeacherCommand>
{
    private readonly IApplicationDbContext _db;

    public AssignGroupTeacherHandler(IApplicationDbContext db) => _db = db;

    public async Task Handle(AssignGroupTeacherCommand request, CancellationToken ct)
    {
        var group = await _db.Groups.FirstOrDefaultAsync(g => g.Id == request.GroupId, ct)
            ?? throw new NotFoundException($"Group '{request.GroupId}' not found.");

        if (request.TeacherId.HasValue)
        {
            var teacher = await _db.Users.FirstOrDefaultAsync(
                u => u.Id == request.TeacherId.Value && u.OrganizationId == group.OrganizationId, ct)
                ?? throw new NotFoundException($"Teacher '{request.TeacherId}' not found.");

            if (teacher.Role != Role.Teacher)
                throw new ConflictException("Selected user is not a Teacher.");
        }

        group.AssignTeacher(request.TeacherId);
        await _db.SaveChangesAsync(ct);
    }
}
