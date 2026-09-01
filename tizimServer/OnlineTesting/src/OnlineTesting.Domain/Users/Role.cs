namespace OnlineTesting.Domain.Users;

public enum Role
{
    // Platform-level: operate the SaaS itself, cross-tenant. OrganizationId is always null.
    Owner = 1,
    SuperAdmin = 2,

    // Org-level: always scoped to exactly one tenant. OrganizationId is always non-null.
    OrgAdmin = 3,
    Teacher = 4, // reserved for the future LMS phase
    Student = 5, // reserved for the future LMS phase
    Staff = 6    // reserved for future CRM staff roles (sales/managers)
}

public static class RoleExtensions
{
    public static bool IsPlatformLevel(this Role role) => role is Role.Owner or Role.SuperAdmin;
}
