using MediatR;
using Microsoft.EntityFrameworkCore;
using OnlineTesting.Application.Common.Exceptions;
using OnlineTesting.Application.Common.Interfaces;

namespace OnlineTesting.Application.Crm.Students.Commands.UpdateStudent;

public class UpdateStudentHandler : IRequestHandler<UpdateStudentCommand>
{
    private readonly IApplicationDbContext _db;

    public UpdateStudentHandler(IApplicationDbContext db) => _db = db;

    public async Task Handle(UpdateStudentCommand request, CancellationToken ct)
    {
        var student = await _db.Students.FirstOrDefaultAsync(s => s.Id == request.Id, ct)
            ?? throw new NotFoundException($"Student '{request.Id}' not found.");

        student.UpdateContactInfo(request.FullName, request.Phone, request.Email);
        student.UpdateNotes(request.Notes);
        await _db.SaveChangesAsync(ct);
    }
}
