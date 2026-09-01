namespace OnlineTesting.Domain.Authorization;

public static class Roles
{
    public const string Owner = nameof(Owner);
    public const string SuperAdmin = nameof(SuperAdmin);
    public const string OrgAdmin = nameof(OrgAdmin);
    public const string Teacher = nameof(Teacher);
    public const string Student = nameof(Student);
    public const string Staff = nameof(Staff);

    public static class Policies
    {
        // Owner-only: platform billing/ops config.
        public const string OwnerAccess = nameof(OwnerAccess);

        // Either platform role. Placeholder for a future cross-tenant platform admin panel.
        public const string PlatformAccess = nameof(PlatformAccess);

        // Platform roles + the org's own OrgAdmin — "manage my organization" actions.
        public const string OrgAdminAccess = nameof(OrgAdminAccess);

        // Platform roles + any org-level staff — day-to-day CRM usage (not owner-only).
        public const string CrmAccess = nameof(CrmAccess);

        // Platform roles + Staff (manage) + Teacher (read own groups only, enforced in handlers).
        public const string GroupsAccess = nameof(GroupsAccess);
    }
}
