using OnlineTesting.Domain.Common;

namespace OnlineTesting.Domain.Crm;

public class Payment : Entity, ITenantScoped
{
    public Guid OrganizationId { get; private set; }
    public Guid GroupId { get; private set; }
    public Guid StudentId { get; private set; }
    public decimal Amount { get; private set; }
    public DateOnly PaidAt { get; private set; }
    public DateOnly ForMonth { get; private set; }
    public PaymentMethod Method { get; private set; }
    public string? Note { get; private set; }
    public Guid RecordedByUserId { get; private set; }
    public DateTime CreatedAt { get; private set; }

    private Payment() { }

    public static Payment Create(
        Guid organizationId, Guid groupId, Guid studentId, decimal amount,
        DateOnly paidAt, DateOnly forMonth, PaymentMethod method, Guid recordedByUserId, string? note = null)
    {
        if (amount <= 0)
            throw new ArgumentException("Amount must be greater than zero.", nameof(amount));

        return new Payment
        {
            Id = Guid.NewGuid(),
            OrganizationId = organizationId,
            GroupId = groupId,
            StudentId = studentId,
            Amount = amount,
            PaidAt = paidAt,
            ForMonth = new DateOnly(forMonth.Year, forMonth.Month, 1),
            Method = method,
            Note = string.IsNullOrWhiteSpace(note) ? null : note.Trim(),
            RecordedByUserId = recordedByUserId,
            CreatedAt = DateTime.UtcNow
        };
    }
}
