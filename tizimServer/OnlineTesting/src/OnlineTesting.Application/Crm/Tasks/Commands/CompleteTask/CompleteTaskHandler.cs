using MediatR;
using Microsoft.EntityFrameworkCore;
using OnlineTesting.Application.Common.Exceptions;
using OnlineTesting.Application.Common.Interfaces;

namespace OnlineTesting.Application.Crm.Tasks.Commands.CompleteTask;

public class CompleteTaskHandler : IRequestHandler<CompleteTaskCommand>
{
    private readonly IApplicationDbContext _db;

    public CompleteTaskHandler(IApplicationDbContext db) => _db = db;

    public async Task Handle(CompleteTaskCommand request, CancellationToken ct)
    {
        var task = await _db.CrmTasks.FirstOrDefaultAsync(t => t.Id == request.Id, ct)
            ?? throw new NotFoundException($"Task '{request.Id}' not found.");

        task.Complete();
        await _db.SaveChangesAsync(ct);
    }
}
