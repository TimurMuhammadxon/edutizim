using OnlineTesting.Domain.Common;

namespace OnlineTesting.Domain.Organizations;

public class Organization : Entity
{
    public string Name { get; private set; } = default!;
    public string Slug { get; private set; } = default!;
    public Guid? OwnerUserId { get; private set; }
    public bool IsActive { get; private set; }
    public DateTime CreatedAt { get; private set; }

    private Organization() { }

    public static Organization Create(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Organization name is required.", nameof(name));

        var id = Guid.NewGuid();

        return new Organization
        {
            Id = id,
            Name = name.Trim(),
            Slug = Slugify(name.Trim()) + "-" + id.ToString("N")[..8],
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
    }

    public void SetOwner(Guid userId) => OwnerUserId = userId;

    public void Rename(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Organization name is required.", nameof(name));
        Name = name.Trim();
    }

    public void Deactivate() => IsActive = false;
    public void Activate() => IsActive = true;

    private static string Slugify(string value)
    {
        var lowered = value.ToLowerInvariant();
        var chars = lowered.Select(c => char.IsLetterOrDigit(c) ? c : '-').ToArray();
        var collapsed = new string(chars);
        while (collapsed.Contains("--"))
            collapsed = collapsed.Replace("--", "-");
        return collapsed.Trim('-');
    }
}
