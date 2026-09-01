using MediatR;
using Microsoft.EntityFrameworkCore;
using OnlineTesting.Application.Common.Exceptions;
using OnlineTesting.Application.Common.Interfaces;
using OnlineTesting.Domain.Crm;

namespace OnlineTesting.Application.Crm.Leads.Commands.ChangeLeadStage;

public class ChangeLeadStageHandler : IRequestHandler<ChangeLeadStageCommand>
{
    private readonly IApplicationDbContext _db;

    public ChangeLeadStageHandler(IApplicationDbContext db) => _db = db;

    public async Task Handle(ChangeLeadStageCommand request, CancellationToken ct)
    {
        var lead = await _db.Leads.FirstOrDefaultAsync(l => l.Id == request.Id, ct)
            ?? throw new NotFoundException($"Lead '{request.Id}' not found.");

        if (request.Stage == LeadStage.Lost)
            lead.MarkLost(request.LostReason);
        else
            lead.ChangeStage(request.Stage);

        await _db.SaveChangesAsync(ct);
    }
}
