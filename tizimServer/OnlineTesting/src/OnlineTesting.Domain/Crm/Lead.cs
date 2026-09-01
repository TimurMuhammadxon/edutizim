using OnlineTesting.Domain.Common;

namespace OnlineTesting.Domain.Crm;

public class Lead : Entity, ITenantScoped
{
    public Guid OrganizationId { get; private set; }
    public Guid BranchId { get; private set; }
    public string FullName { get; private set; } = default!;
    public string Phone { get; private set; } = default!;
    public string? Email { get; private set; }
    public ClientSource Source { get; private set; }
    public LeadStage Stage { get; private set; }
    public Guid? AssignedManagerId { get; private set; }
    public string? Notes { get; private set; }
    public string? LostReason { get; private set; }
    public DateTime CreatedAt { get; private set; }

    private Lead() { }

    public static Lead Create(
        Guid organizationId,
        Guid branchId,
        string fullName,
        string phone,
        ClientSource source,
        Guid? assignedManagerId = null)
    {
        if (string.IsNullOrWhiteSpace(fullName))
            throw new ArgumentException("Full name is required.", nameof(fullName));
        if (string.IsNullOrWhiteSpace(phone))
            throw new ArgumentException("Phone is required.", nameof(phone));

        return new Lead
        {
            Id = Guid.NewGuid(),
            OrganizationId = organizationId,
            BranchId = branchId,
            FullName = fullName.Trim(),
            Phone = phone.Trim(),
            Source = source,
            Stage = LeadStage.New,
            AssignedManagerId = assignedManagerId,
            CreatedAt = DateTime.UtcNow
        };
    }

    public void UpdateContactInfo(string fullName, string phone, string? email)
    {
        if (string.IsNullOrWhiteSpace(fullName))
            throw new ArgumentException("Full name is required.", nameof(fullName));
        if (string.IsNullOrWhiteSpace(phone))
            throw new ArgumentException("Phone is required.", nameof(phone));

        FullName = fullName.Trim();
        Phone = phone.Trim();
        Email = string.IsNullOrWhiteSpace(email) ? null : email.Trim();
    }

    public void ChangeStage(LeadStage stage)
    {
        Stage = stage;
        if (stage != LeadStage.Lost) LostReason = null;
    }

    public void MarkLost(string? reason)
    {
        Stage = LeadStage.Lost;
        LostReason = string.IsNullOrWhiteSpace(reason) ? null : reason.Trim();
    }

    public void AssignManager(Guid? managerId) => AssignedManagerId = managerId;

    public void UpdateNotes(string? notes) => Notes = string.IsNullOrWhiteSpace(notes) ? null : notes.Trim();
}
