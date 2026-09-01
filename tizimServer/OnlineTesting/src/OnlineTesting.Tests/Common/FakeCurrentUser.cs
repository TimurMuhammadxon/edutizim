using OnlineTesting.Application.Common.Interfaces;
using OnlineTesting.Domain.Users;

namespace OnlineTesting.Tests.Common;

public class FakeCurrentUser : ICurrentUser
{
    public Guid? UserId { get; set; }
    public bool IsAuthenticated { get; set; } = true;
    public Guid? OrganizationId { get; set; }
    public Role? Role { get; set; }

    public static FakeCurrentUser ForOrg(Guid organizationId, Role role = OnlineTesting.Domain.Users.Role.OrgAdmin) => new()
    {
        UserId = Guid.NewGuid(),
        OrganizationId = organizationId,
        Role = role,
    };
}
