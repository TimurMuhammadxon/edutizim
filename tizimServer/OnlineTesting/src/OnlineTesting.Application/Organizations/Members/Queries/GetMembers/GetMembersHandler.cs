using MediatR;
using Microsoft.EntityFrameworkCore;
using OnlineTesting.Application.Common.Exceptions;
using OnlineTesting.Application.Common.Interfaces;
using OnlineTesting.Domain.Users;

namespace OnlineTesting.Application.Organizations.Members.Queries.GetMembers;

public class GetMembersHandler : IRequestHandler<GetMembersQuery, List<MemberDto>>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUser _currentUser;

    public GetMembersHandler(IApplicationDbContext db, ICurrentUser currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<List<MemberDto>> Handle(GetMembersQuery request, CancellationToken ct)
    {
        var organizationId = _currentUser.OrganizationId
            ?? throw new UnauthorizedException("User does not belong to an organization.");

        var query = _db.Users.AsNoTracking()
            .Where(u => u.OrganizationId == organizationId && (u.Role == Role.Staff || u.Role == Role.Teacher));

        if (request.Role.HasValue)
            query = query.Where(u => u.Role == request.Role.Value);

        return await query
            .OrderByDescending(u => u.CreatedAt)
            .Select(u => new MemberDto(
                u.Id,
                (u.FirstName ?? "") + (u.LastName != null ? " " + u.LastName : ""),
                u.Phone,
                u.Role.ToString(),
                u.IsActive,
                u.CreatedAt))
            .ToListAsync(ct);
    }
}
