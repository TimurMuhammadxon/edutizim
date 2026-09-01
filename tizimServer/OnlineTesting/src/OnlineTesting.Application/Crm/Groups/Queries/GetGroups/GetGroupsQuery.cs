using MediatR;
using OnlineTesting.Application.Common.Models;

namespace OnlineTesting.Application.Crm.Groups.Queries.GetGroups;

public record GetGroupsQuery(
    Guid? BranchId,
    Guid? TeacherId,
    bool? IsActive,
    int Page = 1,
    int PageSize = 50) : IRequest<PagedResult<GroupDto>>;

public record GroupDto(
    Guid Id,
    Guid BranchId,
    string Name,
    string? Description,
    decimal Price,
    Guid? TeacherId,
    string? TeacherName,
    Guid? RoomId,
    string? RoomName,
    int StudentCount,
    bool IsActive,
    DateTime CreatedAt);
