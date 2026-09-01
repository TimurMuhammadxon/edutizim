using OnlineTesting.Domain.Common;

namespace OnlineTesting.Domain.Organizations;

public class Room : Entity, ITenantScoped
{
    public Guid OrganizationId { get; private set; }
    public Guid BranchId { get; private set; }
    public string Name { get; private set; } = default!;
    public int Capacity { get; private set; }
    public bool IsActive { get; private set; }
    public DateTime CreatedAt { get; private set; }

    private Room() { }

    public static Room Create(Guid organizationId, Guid branchId, string name, int capacity)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Room name is required.", nameof(name));
        if (capacity <= 0)
            throw new ArgumentException("Capacity must be greater than zero.", nameof(capacity));

        return new Room
        {
            Id = Guid.NewGuid(),
            OrganizationId = organizationId,
            BranchId = branchId,
            Name = name.Trim(),
            Capacity = capacity,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
    }

    public void Update(string name, int capacity)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Room name is required.", nameof(name));
        if (capacity <= 0)
            throw new ArgumentException("Capacity must be greater than zero.", nameof(capacity));

        Name = name.Trim();
        Capacity = capacity;
    }

    public void Activate() => IsActive = true;
    public void Deactivate() => IsActive = false;
}
