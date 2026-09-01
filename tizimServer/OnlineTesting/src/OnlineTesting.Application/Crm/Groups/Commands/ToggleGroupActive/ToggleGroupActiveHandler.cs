using MediatR;
using Microsoft.EntityFrameworkCore;
using OnlineTesting.Application.Common.Exceptions;
using OnlineTesting.Application.Common.Interfaces;

namespace OnlineTesting.Application.Crm.Groups.Commands.ToggleGroupActive;

public class ToggleGroupActiveHandler : IRequestHandler<ToggleGroupActiveCommand>
{
    private readonly IApplicationDbContext _db;

    public ToggleGroupActiveHandler(IApplicationDbContext db) => _db = db;

    public async Task Handle(ToggleGroupActiveCommand request, CancellationToken ct)
    {
        var group = await _db.Groups.FirstOrDefaultAsync(g => g.Id == request.Id, ct)
            ?? throw new NotFoundException($"Group '{request.Id}' not found.");

        if (request.IsActive) group.Activate();
        else group.Deactivate();

        await _db.SaveChangesAsync(ct);
    }
}
