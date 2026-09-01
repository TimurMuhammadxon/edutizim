using MediatR;
using Microsoft.EntityFrameworkCore;
using OnlineTesting.Application.Common.Exceptions;
using OnlineTesting.Application.Common.Interfaces;
using OnlineTesting.Domain.Crm;

namespace OnlineTesting.Application.Crm.Leads.Commands.CreateLead;

public class CreateLeadHandler : IRequestHandler<CreateLeadCommand, Guid>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUser _currentUser;

    public CreateLeadHandler(IApplicationDbContext db, ICurrentUser currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<Guid> Handle(CreateLeadCommand request, CancellationToken ct)
    {
        var organizationId = _currentUser.OrganizationId
            ?? throw new UnauthorizedException("User does not belong to an organization.");

        var branchExists = await _db.Branches.AnyAsync(b => b.Id == request.BranchId, ct);
        if (!branchExists)
            throw new NotFoundException($"Branch '{request.BranchId}' not found.");

        var lead = Lead.Create(
            organizationId, request.BranchId, request.FullName, request.Phone, request.Source, request.AssignedManagerId);

        _db.Leads.Add(lead);
        await _db.SaveChangesAsync(ct);

        return lead.Id;
    }
}
