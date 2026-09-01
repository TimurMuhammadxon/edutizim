namespace OnlineTesting.Domain.Common;

public interface ITenantScoped
{
    Guid OrganizationId { get; }
}
