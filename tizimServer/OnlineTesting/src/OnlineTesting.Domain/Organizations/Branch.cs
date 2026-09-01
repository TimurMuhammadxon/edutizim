using OnlineTesting.Domain.Common;

namespace OnlineTesting.Domain.Organizations;

public class Branch : Entity, ITenantScoped
{
    public Guid OrganizationId { get; private set; }
    public string Name { get; private set; } = default!;
    public string? Address { get; private set; }
    public bool IsActive { get; private set; }
    public DateTime CreatedAt { get; private set; }

    private Branch() { }

    public static Branch Create(Guid organizationId, string name, string? address = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Branch name is required.", nameof(name));

        return new Branch
        {
            Id = Guid.NewGuid(),
            OrganizationId = organizationId,
            Name = name.Trim(),
            Address = string.IsNullOrWhiteSpace(address) ? null : address.Trim(),
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
    }

    public void Update(string name, string? address)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Branch name is required.", nameof(name));

        Name = name.Trim();
        Address = string.IsNullOrWhiteSpace(address) ? null : address.Trim();
    }

    public void Activate() => IsActive = true;
    public void Deactivate() => IsActive = false;
}
