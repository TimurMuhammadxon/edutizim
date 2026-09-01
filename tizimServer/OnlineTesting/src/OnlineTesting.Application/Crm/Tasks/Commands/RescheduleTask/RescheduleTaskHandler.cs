using MediatR;
using Microsoft.EntityFrameworkCore;
using OnlineTesting.Application.Common.Exceptions;
using OnlineTesting.Application.Common.Interfaces;

namespace OnlineTesting.Application.Crm.Tasks.Commands.RescheduleTask;

public class RescheduleTaskHandler : IRequestHandler<RescheduleTaskCommand>
{
    private readonly IApplicationDbContext _db;

    public RescheduleTaskHandler(IApplicationDbContext db) => _db = db;

    public async Task Handle(RescheduleTaskCommand request, CancellationToken ct)
    {
        var task = await _db.CrmTasks.FirstOrDefaultAsync(t => t.Id == request.Id, ct)
            ?? throw new NotFoundException($"Task '{request.Id}' not found.");

        task.Reschedule(request.DueAt);
        await _db.SaveChangesAsync(ct);
    }
}
