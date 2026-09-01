using MediatR;
using Microsoft.EntityFrameworkCore;
using OnlineTesting.Application.Common.Exceptions;
using OnlineTesting.Application.Common.Interfaces;

namespace OnlineTesting.Application.Crm.Students.Commands.ToggleStudentActive;

public class ToggleStudentActiveHandler : IRequestHandler<ToggleStudentActiveCommand>
{
    private readonly IApplicationDbContext _db;

    public ToggleStudentActiveHandler(IApplicationDbContext db) => _db = db;

    public async Task Handle(ToggleStudentActiveCommand request, CancellationToken ct)
    {
        var student = await _db.Students.FirstOrDefaultAsync(s => s.Id == request.Id, ct)
            ?? throw new NotFoundException($"Student '{request.Id}' not found.");

        if (request.IsActive) student.Activate();
        else student.Deactivate();

        await _db.SaveChangesAsync(ct);
    }
}
