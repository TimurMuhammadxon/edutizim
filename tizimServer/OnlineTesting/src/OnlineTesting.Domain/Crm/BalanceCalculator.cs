namespace OnlineTesting.Domain.Crm;

public static class BalanceCalculator
{
    /// Safety cap on how many calendar months either loop below will walk — guards against a
    /// pathological/corrupt totalPaid value (or a zero-priced group) spinning forever.
    private const int MaxMonths = 1200; // 100 years

    /// Billing is per calendar month, not a rolling anniversary of the activation date: a student
    /// activated on any day of September owes September's charge (via priceForMonth), October's
    /// charge on 1 October, and so on. priceForMonth lets the caller apply a discount that only
    /// overlaps specific month(s) — e.g. how staff manually re-price a partial first month.
    ///
    /// A month is only ever charged once it has fully elapsed relative to `today` — the current
    /// calendar month is never counted as owed, no matter how many days into it we are or whether
    /// it's been paid yet. A student only becomes a debtor once a month has closed unpaid (i.e. the
    /// next month has started). `asOfDate` (typically `today`, or the freeze date while frozen) can
    /// still cap billing earlier than that, e.g. to stop new charges while a membership is frozen.
    ///
    /// The overall Balance is a simple aggregate (all money received vs. all fully-elapsed months'
    /// charges) — which month a payment was actually earmarked for doesn't change how much is owed in
    /// total. NextDueDate is different: it's "which specific elapsed month is still short," so it's
    /// derived from paidForMonth — the amount staff explicitly tagged to each month when recording
    /// payments — rather than assumed FIFO fill order.
    public static (decimal Balance, DateOnly? NextDueDate) Compute(
        DateTime? activatedAt, decimal totalPaid,
        Func<DateOnly, decimal> priceForMonth, Func<DateOnly, decimal> paidForMonth,
        DateOnly asOfDate, DateOnly today)
    {
        if (activatedAt is null)
            return (0m, null);

        var startMonth = new DateOnly(activatedAt.Value.Year, activatedAt.Value.Month, 1);
        var asOfMonth = new DateOnly(asOfDate.Year, asOfDate.Month, 1);
        var lastElapsedMonth = new DateOnly(today.Year, today.Month, 1).AddMonths(-1);
        var chargeableMonth = asOfMonth < lastElapsedMonth ? asOfMonth : lastElapsedMonth;

        var expectedCharged = 0m;
        var month = startMonth;
        for (var i = 0; month <= chargeableMonth && i < MaxMonths; i++, month = month.AddMonths(1))
            expectedCharged += priceForMonth(month);

        var balance = totalPaid - expectedCharged;

        DateOnly? nextDueDate = null;
        month = startMonth;
        for (var i = 0; month <= chargeableMonth && i < MaxMonths; i++, month = month.AddMonths(1))
        {
            if (paidForMonth(month) < priceForMonth(month))
            {
                nextDueDate = month;
                break;
            }
        }

        return (balance, nextDueDate);
    }
}
