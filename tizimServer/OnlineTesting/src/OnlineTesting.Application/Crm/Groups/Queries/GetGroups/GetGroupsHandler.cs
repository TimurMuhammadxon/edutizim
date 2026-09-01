using MediatR;
using Microsoft.EntityFrameworkCore;
using OnlineTesting.Application.Common.Interfaces;
using OnlineTesting.Application.Common.Models;
using OnlineTesting.Domain.Users;

namespace OnlineTesting.Application.Crm.Groups.Queries.GetGroups;

public class GetGroupsHandler : IRequestHandler<GetGroupsQuery, PagedResult<GroupDto>>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUser _currentUser;

    public GetGroupsHandler(IApplicationDbContext db, ICurrentUser currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<PagedResult<GroupDto>> Handle(GetGroupsQuery request, CancellationToken ct)
    {
        var page = Math.Max(1, request.Page);
        var size = Math.Clamp(request.PageSize, 1, 200);

        // A Teacher can only ever see their own groups, regardless of what TeacherId was requested.
        var teacherFilter = _currentUser.Role == Role.Teacher ? _currentUser.UserId : request.TeacherId;

        var query =
            from g in _db.Groups.AsNoTracking()
            join t in _db.Users.AsNoTracking() on g.TeacherId equals (Guid?)t.Id into teacherJoin
            from t in teacherJoin.DefaultIfEmpty()
            join r in _db.Rooms.AsNoTracking() on g.RoomId equals (Guid?)r.Id into roomJoin
            from r in roomJoin.DefaultIfEmpty()
            select new
            {
                Group = g,
                TeacherName = t == null ? null : ((t.FirstName ?? "") + (t.LastName != null ? " " + t.LastName : "")),
                RoomName = r == null ? null : r.Name,
                StudentCount = _db.GroupStudents.Count(gs => gs.GroupId == g.Id)
            };

        if (request.BranchId.HasValue)
            query = query.Where(x => x.Group.BranchId == request.BranchId.Value);

        if (teacherFilter.HasValue)
            query = query.Where(x => x.Group.TeacherId == teacherFilter.Value);

        if (request.IsActive.HasValue)
            query = query.Where(x => x.Group.IsActive == request.IsActive.Value);

        var total = await query.CountAsync(ct);

        var rows = await query
            .OrderByDescending(x => x.Group.CreatedAt)
            .Skip((page - 1) * size)
            .Take(size)
            .ToListAsync(ct);

        var items = rows.Select(x => new GroupDto(
            x.Group.Id, x.Group.BranchId, x.Group.Name, x.Group.Description, x.Group.Price,
            x.Group.TeacherId, x.TeacherName, x.Group.RoomId, x.RoomName,
            x.StudentCount, x.Group.IsActive, x.Group.CreatedAt))
            .ToList();

        return new PagedResult<GroupDto>(items, page, size, total);
    }
}
