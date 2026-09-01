using MediatR;
using Microsoft.EntityFrameworkCore;
using OnlineTesting.Application.Common.Exceptions;
using OnlineTesting.Application.Common.Interfaces;

namespace OnlineTesting.Application.Crm.Leads.Commands.UpdateLead;

public class UpdateLeadHandler : IRequestHandler<UpdateLeadCommand>
{
    private readonly IApplicationDbContext _db;

    public UpdateLeadHandler(IApplicationDbContext db) => _db = db;

    public async Task Handle(UpdateLeadCommand request, CancellationToken ct)
    {
        var lead = await _db.Leads.FirstOrDefaultAsync(l => l.Id == request.Id, ct)
            ?? throw new NotFoundException($"Lead '{request.Id}' not found.");

        lead.UpdateContactInfo(request.FullName, request.Phone, request.Email);
        lead.UpdateNotes(request.Notes);
        await _db.SaveChangesAsync(ct);
    }
}
