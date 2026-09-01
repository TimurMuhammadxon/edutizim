using MediatR;
using OnlineTesting.Domain.Users;

namespace OnlineTesting.Application.Organizations.Members.Queries.GetMembers;

public record GetMembersQuery(Role? Role) : IRequest<List<MemberDto>>;

public record MemberDto(
    Guid Id,
    string? FullName,
    string? Phone,
    string Role,
    bool IsActive,
    DateTime CreatedAt);
