namespace OnlineTesting.Domain.Crm;

public class GroupStudent
{
    public Guid GroupId { get; private set; }
    public Guid StudentId { get; private set; }
    public DateTime JoinedAt { get; private set; }
    public GroupMembershipStatus Status { get; private set; }
    public DateTime? ActivatedAt { get; private set; }
    public DateTime? FrozenAt { get; private set; }
    public decimal? DiscountedPrice { get; private set; }
    public DateOnly? DiscountStartDate { get; private set; }
    public DateOnly? DiscountEndDate { get; private set; }

    private GroupStudent() { }

    public static GroupStudent Create(Guid groupId, Guid studentId)
    {
        return new GroupStudent
        {
            GroupId = groupId,
            StudentId = studentId,
            JoinedAt = DateTime.UtcNow,
            Status = GroupMembershipStatus.Trial
        };
    }

    public void RecordPayment()
    {
        if (Status == GroupMembershipStatus.Trial)
        {
            Status = GroupMembershipStatus.Active;
            ActivatedAt = DateTime.UtcNow;
        }
    }

    public void Freeze()
    {
        Status = GroupMembershipStatus.Frozen;
        FrozenAt = DateTime.UtcNow;
    }

    public void Unfreeze()
    {
        if (FrozenAt.HasValue && ActivatedAt.HasValue)
            ActivatedAt = ActivatedAt.Value.Add(DateTime.UtcNow - FrozenAt.Value);

        FrozenAt = null;
        Status = GroupMembershipStatus.Active;
    }

    public void MarkLeft() => Status = GroupMembershipStatus.Left;

    public void SetDiscount(decimal price, DateOnly start, DateOnly end)
    {
        if (price < 0)
            throw new ArgumentException("Discounted price cannot be negative.", nameof(price));
        if (end <= start)
            throw new ArgumentException("End date must be after start date.", nameof(end));

        DiscountedPrice = price;
        DiscountStartDate = start;
        DiscountEndDate = end;
    }

    public void RemoveDiscount()
    {
        DiscountedPrice = null;
        DiscountStartDate = null;
        DiscountEndDate = null;
    }

    public decimal EffectivePrice(decimal groupPrice, DateOnly today) =>
        EffectivePriceForMonth(new DateOnly(today.Year, today.Month, 1), groupPrice);

    /// A discount overrides the price for whichever calendar month(s) it overlaps — this is how
    /// staff manually re-price a partial month (e.g. a student joining mid-month) instead of the
    /// system auto-prorating: set a discount whose date range covers just that one month.
    public decimal EffectivePriceForMonth(DateOnly monthStart, decimal groupPrice)
    {
        if (DiscountedPrice.HasValue && DiscountStartDate.HasValue && DiscountEndDate.HasValue)
        {
            var monthEnd = monthStart.AddMonths(1).AddDays(-1);
            if (DiscountStartDate.Value <= monthEnd && DiscountEndDate.Value >= monthStart)
                return DiscountedPrice.Value;
        }

        return groupPrice;
    }

    public DateOnly BalanceAsOfDate(DateOnly today)
    {
        if (Status == GroupMembershipStatus.Frozen && FrozenAt.HasValue)
        {
            var frozenDate = DateOnly.FromDateTime(FrozenAt.Value);
            return frozenDate < today ? frozenDate : today;
        }

        return today;
    }
}
