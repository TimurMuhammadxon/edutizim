using MediatR;
using Microsoft.EntityFrameworkCore;
using OnlineTesting.Application.Common.Exceptions;
using OnlineTesting.Application.Common.Interfaces;

namespace OnlineTesting.Application.Crm.Leads.Commands.AssignLeadManager;

public class AssignLeadManagerHandler : IRequestHandler<AssignLeadManagerCommand>
{
    private readonly IApplicationDbContext _db;

    public AssignLeadManagerHandler(IApplicationDbContext db) => _db = db;

    public async Task Handle(AssignLeadManagerCommand request, CancellationToken ct)
    {
        var lead = await _db.Leads.FirstOrDefaultAsync(l => l.Id == request.Id, ct)
            ?? throw new NotFoundException($"Lead '{request.Id}' not found.");

        lead.AssignManager(request.ManagerId);
        await _db.SaveChangesAsync(ct);
    }
}
