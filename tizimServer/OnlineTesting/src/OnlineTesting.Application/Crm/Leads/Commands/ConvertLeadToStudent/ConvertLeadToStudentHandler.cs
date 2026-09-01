using MediatR;
using Microsoft.EntityFrameworkCore;
using OnlineTesting.Application.Common.Exceptions;
using OnlineTesting.Application.Common.Interfaces;
using OnlineTesting.Domain.Crm;

namespace OnlineTesting.Application.Crm.Leads.Commands.ConvertLeadToStudent;

public class ConvertLeadToStudentHandler : IRequestHandler<ConvertLeadToStudentCommand, Guid>
{
    private readonly IApplicationDbContext _db;

    public ConvertLeadToStudentHandler(IApplicationDbContext db) => _db = db;

    public async Task<Guid> Handle(ConvertLeadToStudentCommand request, CancellationToken ct)
    {
        var lead = await _db.Leads.FirstOrDefaultAsync(l => l.Id == request.Id, ct)
            ?? throw new NotFoundException($"Lead '{request.Id}' not found.");

        if (lead.Stage == LeadStage.Converted)
            throw new ConflictException("Lead is already converted.");

        var student = Student.Create(
            lead.OrganizationId, lead.BranchId, lead.FullName, lead.Phone, lead.Email, lead.Id);

        lead.ChangeStage(LeadStage.Converted);

        _db.Students.Add(student);
        await _db.SaveChangesAsync(ct);

        return student.Id;
    }
}
