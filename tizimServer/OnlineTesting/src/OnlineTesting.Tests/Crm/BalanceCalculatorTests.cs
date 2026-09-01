using OnlineTesting.Domain.Crm;

namespace OnlineTesting.Tests.Crm;

/// BalanceCalculator.Compute is pure and carries the trickiest rule in the CRM finance
/// module — "a calendar month is only charged once it has fully elapsed" plus deriving
/// NextDueDate from per-month paid/price rather than assuming FIFO fill order — so it's
/// exercised directly with no DB/handler involved.
public class BalanceCalculatorTests
{
    private static readonly DateOnly Today = new(2026, 9, 1);

    [Fact]
    public void NullActivatedAt_ReturnsZeroBalanceAndNoDueDate()
    {
        var (balance, dueDate) = BalanceCalculator.Compute(
            activatedAt: null, totalPaid: 100_000,
            priceForMonth: _ => 100_000, paidForMonth: _ => 0,
            asOfDate: Today, today: Today);

        Assert.Equal(0m, balance);
        Assert.Null(dueDate);
    }

    [Fact]
    public void ActivatedThisMonth_NotYetCharged_EvenIfUnpaid()
    {
        var activatedThisMonth = new DateTime(Today.Year, Today.Month, 1);

        var (balance, dueDate) = BalanceCalculator.Compute(
            activatedAt: activatedThisMonth, totalPaid: 0,
            priceForMonth: _ => 100_000, paidForMonth: _ => 0,
            asOfDate: Today, today: Today);

        Assert.Equal(0m, balance);
        Assert.Null(dueDate);
    }

    [Fact]
    public void OneFullyElapsedUnpaidMonth_IsOwed()
    {
        var activatedAt = new DateTime(2026, 8, 1); // fully elapsed by September 1st

        var (balance, dueDate) = BalanceCalculator.Compute(
            activatedAt: activatedAt, totalPaid: 0,
            priceForMonth: _ => 100_000, paidForMonth: _ => 0,
            asOfDate: Today, today: Today);

        Assert.Equal(-100_000m, balance);
        Assert.Equal(new DateOnly(2026, 8, 1), dueDate);
    }

    [Fact]
    public void ElapsedMonthPaidInFull_ZeroBalanceNoDueDate()
    {
        var activatedAt = new DateTime(2026, 8, 1);

        var (balance, dueDate) = BalanceCalculator.Compute(
            activatedAt: activatedAt, totalPaid: 100_000,
            priceForMonth: _ => 100_000, paidForMonth: _ => 100_000,
            asOfDate: Today, today: Today);

        Assert.Equal(0m, balance);
        Assert.Null(dueDate);
    }

    [Fact]
    public void PartialPaymentAcrossTwoMonths_NextDueDateIsFirstShortMonth()
    {
        var activatedAt = new DateTime(2026, 7, 1); // July + August both fully elapsed
        var paid = new Dictionary<DateOnly, decimal>
        {
            [new DateOnly(2026, 7, 1)] = 100_000, // July: paid in full
            [new DateOnly(2026, 8, 1)] = 50_000,  // August: underpaid
        };

        var (balance, dueDate) = BalanceCalculator.Compute(
            activatedAt: activatedAt, totalPaid: 150_000,
            priceForMonth: _ => 100_000, paidForMonth: m => paid.GetValueOrDefault(m),
            asOfDate: Today, today: Today);

        Assert.Equal(-50_000m, balance);
        Assert.Equal(new DateOnly(2026, 8, 1), dueDate);
    }

    [Fact]
    public void FrozenBeforeMonthElapsed_AsOfDateCapsBilling()
    {
        // Membership frozen mid-July: August has since fully elapsed relative to `today`,
        // but billing must stop at the freeze date, not follow the wall clock.
        var activatedAt = new DateTime(2026, 7, 1);
        var frozenAt = new DateOnly(2026, 7, 15);

        var (balance, dueDate) = BalanceCalculator.Compute(
            activatedAt: activatedAt, totalPaid: 0,
            priceForMonth: _ => 100_000, paidForMonth: _ => 0,
            asOfDate: frozenAt, today: Today);

        Assert.Equal(-100_000m, balance); // July only, not August
        Assert.Equal(new DateOnly(2026, 7, 1), dueDate);
    }

    [Fact]
    public void DiscountedMonth_UsesPerMonthPrice()
    {
        var activatedAt = new DateTime(2026, 7, 1); // July + August elapsed
        var prices = new Dictionary<DateOnly, decimal>
        {
            [new DateOnly(2026, 7, 1)] = 100_000,
            [new DateOnly(2026, 8, 1)] = 50_000, // discounted month
        };

        var (balance, dueDate) = BalanceCalculator.Compute(
            activatedAt: activatedAt, totalPaid: 0,
            priceForMonth: m => prices[m], paidForMonth: _ => 0,
            asOfDate: Today, today: Today);

        Assert.Equal(-150_000m, balance);
        Assert.Equal(new DateOnly(2026, 7, 1), dueDate); // still the first short month
    }
}
