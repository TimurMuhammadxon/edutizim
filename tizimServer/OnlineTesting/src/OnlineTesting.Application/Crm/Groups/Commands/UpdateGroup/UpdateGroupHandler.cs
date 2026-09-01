using MediatR;
using Microsoft.EntityFrameworkCore;
using OnlineTesting.Application.Common.Exceptions;
using OnlineTesting.Application.Common.Interfaces;

namespace OnlineTesting.Application.Crm.Groups.Commands.UpdateGroup;

public class UpdateGroupHandler : IRequestHandler<UpdateGroupCommand>
{
    private readonly IApplicationDbContext _db;

    public UpdateGroupHandler(IApplicationDbContext db) => _db = db;

    public async Task Handle(UpdateGroupCommand request, CancellationToken ct)
    {
        var group = await _db.Groups.FirstOrDefaultAsync(g => g.Id == request.Id, ct)
            ?? throw new NotFoundException($"Group '{request.Id}' not found.");

        group.Update(request.Name, request.Price, request.Description);
        await _db.SaveChangesAsync(ct);
    }
}
