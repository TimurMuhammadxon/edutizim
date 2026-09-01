using MediatR;
using Microsoft.EntityFrameworkCore;
using OnlineTesting.Application.Common.Exceptions;
using OnlineTesting.Application.Common.Interfaces;
using OnlineTesting.Application.Crm.Leads.Queries.GetLeads;

namespace OnlineTesting.Application.Crm.Leads.Queries.GetLeadById;

public class GetLeadByIdHandler : IRequestHandler<GetLeadByIdQuery, LeadDto>
{
    private readonly IApplicationDbContext _db;

    public GetLeadByIdHandler(IApplicationDbContext db) => _db = db;

    public async Task<LeadDto> Handle(GetLeadByIdQuery request, CancellationToken ct)
    {
        var lead = await _db.Leads.AsNoTracking()
            .FirstOrDefaultAsync(l => l.Id == request.Id, ct)
            ?? throw new NotFoundException($"Lead '{request.Id}' not found.");

        return new LeadDto(
            lead.Id, lead.BranchId, lead.FullName, lead.Phone, lead.Email, lead.Source.ToString(), lead.Stage.ToString(),
            lead.AssignedManagerId, lead.Notes, lead.LostReason, lead.CreatedAt);
    }
}
