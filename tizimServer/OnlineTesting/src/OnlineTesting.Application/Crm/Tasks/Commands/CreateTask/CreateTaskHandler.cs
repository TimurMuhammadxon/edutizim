using MediatR;
using Microsoft.EntityFrameworkCore;
using OnlineTesting.Application.Common.Exceptions;
using OnlineTesting.Application.Common.Interfaces;
using OnlineTesting.Domain.Crm;

namespace OnlineTesting.Application.Crm.Tasks.Commands.CreateTask;

public class CreateTaskHandler : IRequestHandler<CreateTaskCommand, Guid>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUser _currentUser;

    public CreateTaskHandler(IApplicationDbContext db, ICurrentUser currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<Guid> Handle(CreateTaskCommand request, CancellationToken ct)
    {
        var organizationId = _currentUser.OrganizationId
            ?? throw new UnauthorizedException("User does not belong to an organization.");

        if (request.LeadId.HasValue)
        {
            var leadExists = await _db.Leads.AnyAsync(l => l.Id == request.LeadId.Value, ct);
            if (!leadExists)
                throw new NotFoundException($"Lead '{request.LeadId}' not found.");
        }

        var task = CrmTask.Create(
            organizationId, request.Title, request.DueAt, request.AssignedToUserId,
            request.LeadId, request.Description);

        _db.CrmTasks.Add(task);
        await _db.SaveChangesAsync(ct);

        return task.Id;
    }
}
