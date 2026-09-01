using OnlineTesting.Domain.Common;

namespace OnlineTesting.Domain.Crm;

public class Student : Entity, ITenantScoped
{
    public Guid OrganizationId { get; private set; }
    public Guid BranchId { get; private set; }
    public Guid? LeadId { get; private set; }
    public Guid? UserId { get; private set; }
    public string FullName { get; private set; } = default!;
    public string Phone { get; private set; } = default!;
    public string? Email { get; private set; }
    public bool IsActive { get; private set; }
    public string? Notes { get; private set; }
    public DateTime CreatedAt { get; private set; }

    private Student() { }

    public static Student Create(
        Guid organizationId,
        Guid branchId,
        string fullName,
        string phone,
        string? email = null,
        Guid? leadId = null)
    {
        if (string.IsNullOrWhiteSpace(fullName))
            throw new ArgumentException("Full name is required.", nameof(fullName));
        if (string.IsNullOrWhiteSpace(phone))
            throw new ArgumentException("Phone is required.", nameof(phone));

        return new Student
        {
            Id = Guid.NewGuid(),
            OrganizationId = organizationId,
            BranchId = branchId,
            LeadId = leadId,
            FullName = fullName.Trim(),
            Phone = phone.Trim(),
            Email = string.IsNullOrWhiteSpace(email) ? null : email.Trim(),
            IsActive = true,
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

    public void UpdateNotes(string? notes) => Notes = string.IsNullOrWhiteSpace(notes) ? null : notes.Trim();

    public void LinkUser(Guid userId) => UserId = userId;

    public void Activate() => IsActive = true;
    public void Deactivate() => IsActive = false;
}
