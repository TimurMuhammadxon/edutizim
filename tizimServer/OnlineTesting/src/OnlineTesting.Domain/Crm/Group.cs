using OnlineTesting.Domain.Common;

namespace OnlineTesting.Domain.Crm;

public class Group : Entity, ITenantScoped
{
    public Guid OrganizationId { get; private set; }
    public Guid BranchId { get; private set; }
    public string Name { get; private set; } = default!;
    public string? Description { get; private set; }
    public decimal Price { get; private set; }
    public Guid? TeacherId { get; private set; }
    public Guid? RoomId { get; private set; }
    public bool IsActive { get; private set; }
    public DateTime CreatedAt { get; private set; }

    private Group() { }

    public static Group Create(Guid organizationId, Guid branchId, string name, decimal price, string? description = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Group name is required.", nameof(name));
        if (price < 0)
            throw new ArgumentException("Price cannot be negative.", nameof(price));

        return new Group
        {
            Id = Guid.NewGuid(),
            OrganizationId = organizationId,
            BranchId = branchId,
            Name = name.Trim(),
            Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim(),
            Price = price,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
    }

    public void Update(string name, decimal price, string? description)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Group name is required.", nameof(name));
        if (price < 0)
            throw new ArgumentException("Price cannot be negative.", nameof(price));

        Name = name.Trim();
        Price = price;
        Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim();
    }

    public void AssignTeacher(Guid? teacherId) => TeacherId = teacherId;

    public void AssignRoom(Guid? roomId) => RoomId = roomId;

    public void Activate() => IsActive = true;
    public void Deactivate() => IsActive = false;
}
