using OnlineTesting.Domain.Users;

namespace OnlineTesting.Application.Common.Interfaces;

public interface ICurrentUser
{
    Guid? UserId { get; }
    bool IsAuthenticated { get; }
    Guid? OrganizationId { get; }
    Role? Role { get; }
}
