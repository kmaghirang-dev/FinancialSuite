using FinancialSuite.Application.CreditCard.Dtos;

namespace FinancialSuite.Application.CreditCard.Calculations
{
    // Combines card ledgers into the overview: totals for a date range, per-card history and chart series.
    // Statement periods belong to the range their cutoff (PeriodEndDate) falls in; payments by their payment date.
    public static class CreditCardOverviewCalculator
    {
        public static CreditCardOverviewDto Build(IEnumerable<Domain.Entities.CreditCard> cards, DateOnly? from, DateOnly? to, DateOnly today)
        {
            var ledgers = cards
                .Select(card => (Card: card, Ledger: CreditCardDebtCalculator.Calculate(card)))
                .ToList();

            bool InRange(DateOnly date) => (from == null || date >= from) && (to == null || date <= to);

            var periodsInRange = ledgers.SelectMany(x => x.Ledger.Periods.Where(p => InRange(p.PeriodEndDate))).ToList();
            var paymentsInRange = ledgers.SelectMany(x => x.Ledger.CountedPayments.Where(p => InRange(p.PaymentDate))).ToList();

            var initialTotal = ledgers.Sum(x => x.Ledger.InitialBalance);
            var outstandingTotal = ledgers.Sum(x => x.Ledger.CurrentOutstanding);
            var movement = periodsInRange.Sum(x => x.NetDebtMovement);

            return new CreditCardOverviewDto
            {
                From = from,
                To = to,
                Summary = new CreditCardSummaryDto
                {
                    CardCount = ledgers.Count,
                    TotalOutstanding = outstandingTotal,
                    TotalPayments = paymentsInRange.Sum(x => x.Amount),
                    PaymentCount = paymentsInRange.Count,
                    EstimatedNetActivity = periodsInRange.Sum(x => x.EstimatedNetActivity),
                    CompletedPeriodCount = periodsInRange.Count,
                    NetDebtMovement = movement,
                    DebtStatus = DebtStatus.From(movement),
                    InitialTotalBalance = initialTotal,
                    DebtReduction = initialTotal - outstandingTotal,
                    DebtReductionPercent = CreditCardDebtCalculator.Percent(initialTotal - outstandingTotal, initialTotal)
                },
                Cards = ledgers.Select(x => ToLedgerDto(x.Card, x.Ledger, InRange, to)).ToList(),
                BalanceTrend = BalanceTrend(ledgers, from, to, today),
                PeriodTotals = periodsInRange
                    .GroupBy(x => MonthKey(x.PeriodEndDate))
                    .OrderBy(x => x.Key)
                    .Select(x => new CreditCardPeriodTotalDto
                    {
                        Month = x.Key,
                        PeriodCount = x.Count(),
                        TotalPayments = x.Sum(p => p.TotalPayments),
                        EstimatedNetActivity = x.Sum(p => p.EstimatedNetActivity),
                        NetDebtMovement = x.Sum(p => p.NetDebtMovement)
                    })
                    .ToList()
            };
        }

        public static CreditCardLedgerDto ToLedgerDto(Domain.Entities.CreditCard card, CreditCardLedger ledger, Func<DateOnly, bool>? inRange = null, DateOnly? to = null)
        {
            var periods = inRange == null ? ledger.Periods : ledger.Periods.Where(p => inRange(p.PeriodEndDate)).ToList();
            var latest = ledger.Periods.LastOrDefault();

            return new CreditCardLedgerDto
            {
                CreditCardId = card.CreditCardId,
                Name = card.Name,
                Nickname = card.Nickname,
                DisplayName = DisplayName(card),
                CreditLimit = card.CreditLimit,
                AvailableCredit = card.CreditLimit - ledger.CurrentOutstanding,
                IsActive = card.IsActive,
                InitialBalance = ledger.InitialBalance,
                InitialBalanceDate = ledger.InitialBalanceDate,
                CurrentOutstanding = ledger.CurrentOutstanding,
                LatestStatementBalance = ledger.LatestStatementBalance,
                LatestStatementDate = latest?.StatementDate,
                StatementCount = ledger.Periods.Count,
                DebtReduction = ledger.DebtReduction,
                DebtReductionPercent = ledger.DebtReductionPercent,
                Periods = periods,
                // The open period continues to today, so it is shown unless the range ends before it starts.
                OpenPeriod = to == null || to > ledger.OpenPeriod.SinceDate ? ledger.OpenPeriod : null
            };
        }

        public static string DisplayName(Domain.Entities.CreditCard card)
            => string.IsNullOrWhiteSpace(card.Nickname) ? card.Name : $"{card.Name} ({card.Nickname})";

        // One point per month-end from the first initial balance up to last month, each the sum of every card's latest
        // statement balance at that date, then a "now" point with today's outstanding debt (payments since the latest
        // statement already deducted).
        private static List<CreditCardBalancePointDto> BalanceTrend(List<(Domain.Entities.CreditCard Card, CreditCardLedger Ledger)> ledgers, DateOnly? from, DateOnly? to, DateOnly today)
        {
            var points = new List<CreditCardBalancePointDto>();
            if (ledgers.Count == 0) return points;

            var firstDate = ledgers.Min(x => x.Ledger.InitialBalanceDate);
            var month = new DateOnly(firstDate.Year, firstDate.Month, 1);
            var currentMonth = new DateOnly(today.Year, today.Month, 1);

            while (month < currentMonth)
            {
                var monthEnd = month.AddMonths(1).AddDays(-1);
                var overlapsRange = (from == null || monthEnd >= from) && (to == null || month <= to);
                if (overlapsRange)
                {
                    var tracked = ledgers.Where(x => x.Ledger.InitialBalanceDate <= monthEnd).ToList();
                    points.Add(new CreditCardBalancePointDto
                    {
                        Month = MonthKey(month),
                        Date = monthEnd,
                        Balance = tracked.Sum(x => StatementBalanceAt(x.Ledger, monthEnd)),
                        IsCurrent = false
                    });
                }
                month = month.AddMonths(1);
            }

            if (to == null || to >= today)
            {
                points.Add(new CreditCardBalancePointDto
                {
                    Month = MonthKey(today),
                    Date = today,
                    Balance = ledgers.Sum(x => x.Ledger.CurrentOutstanding),
                    IsCurrent = true
                });
            }

            return points;
        }

        // The latest statement balance known at a date (the initial balance before the first statement).
        public static decimal StatementBalanceAt(CreditCardLedger ledger, DateOnly date)
            => ledger.Periods.LastOrDefault(p => p.PeriodEndDate <= date)?.ClosingBalance ?? ledger.InitialBalance;

        private static string MonthKey(DateOnly date) => date.ToString("yyyy-MM");
    }
}
