using MediatR;
using Microsoft.EntityFrameworkCore;
using OnlineTesting.Application.Common.Exceptions;
using OnlineTesting.Application.Common.Interfaces;

namespace OnlineTesting.Application.Crm.Tasks.Commands.CancelTask;

public class CancelTaskHandler : IRequestHandler<CancelTaskCommand>
{
    private readonly IApplicationDbContext _db;

    public CancelTaskHandler(IApplicationDbContext db) => _db = db;

    public async Task Handle(CancelTaskCommand request, CancellationToken ct)
    {
        var task = await _db.CrmTasks.FirstOrDefaultAsync(t => t.Id == request.Id, ct)
            ?? throw new NotFoundException($"Task '{request.Id}' not found.");

        task.Cancel();
        await _db.SaveChangesAsync(ct);
    }
}
