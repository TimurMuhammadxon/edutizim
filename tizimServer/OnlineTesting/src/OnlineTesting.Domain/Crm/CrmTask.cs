using OnlineTesting.Domain.Common;

namespace OnlineTesting.Domain.Crm;

public class CrmTask : Entity, ITenantScoped
{
    public Guid OrganizationId { get; private set; }
    public string Title { get; private set; } = default!;
    public string? Description { get; private set; }
    public DateTime DueAt { get; private set; }
    public CrmTaskStatus Status { get; private set; }
    public Guid AssignedToUserId { get; private set; }
    public Guid? LeadId { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? CompletedAt { get; private set; }

    private CrmTask() { }

    public static CrmTask Create(
        Guid organizationId,
        string title,
        DateTime dueAt,
        Guid assignedToUserId,
        Guid? leadId = null,
        string? description = null)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Title is required.", nameof(title));

        return new CrmTask
        {
            Id = Guid.NewGuid(),
            OrganizationId = organizationId,
            Title = title.Trim(),
            Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim(),
            DueAt = dueAt,
            Status = CrmTaskStatus.Pending,
            AssignedToUserId = assignedToUserId,
            LeadId = leadId,
            CreatedAt = DateTime.UtcNow
        };
    }

    public void Complete()
    {
        Status = CrmTaskStatus.Done;
        CompletedAt = DateTime.UtcNow;
    }

    public void Cancel()
    {
        Status = CrmTaskStatus.Cancelled;
        CompletedAt = null;
    }

    public void Reschedule(DateTime dueAt) => DueAt = dueAt;

    public void UpdateDetails(string title, string? description)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Title is required.", nameof(title));

        Title = title.Trim();
        Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim();
    }
}
