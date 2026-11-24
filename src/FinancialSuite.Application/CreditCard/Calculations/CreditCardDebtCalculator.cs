namespace FinancialSuite.Application.CreditCard.Calculations
{
    public static class DebtStatus
    {
        public const string Increased = "Increased";
        public const string Decreased = "Decreased";
        public const string NoChange = "No change";

        public static string From(decimal netDebtMovement)
            => netDebtMovement > 0 ? Increased : netDebtMovement < 0 ? Decreased : NoChange;
    }

    public record LedgerStatement(Guid StatementId, DateOnly StatementDate, DateOnly PeriodStartDate, DateOnly PeriodEndDate, decimal ClosingBalance, string Notes = "");

    public record LedgerPayment(Guid PaymentId, DateOnly PaymentDate, decimal Amount);

    // A finished statement-to-statement period: both its opening and its closing statement are known.
    public class StatementPeriod
    {
        public Guid StatementId { get; init; }
        public DateOnly StatementDate { get; init; }
        public DateOnly PeriodStartDate { get; init; }
        public DateOnly PeriodEndDate { get; init; }
        // The cutoff the period is measured from: the previous statement's PeriodEndDate, or the initial balance date.
        public DateOnly PreviousCutoffDate { get; init; }
        public bool OpensFromInitialBalance { get; init; }
        public decimal OpeningBalance { get; init; }
        public decimal TotalPayments { get; init; }
        public int PaymentCount { get; init; }
        // Closing - opening + payments: purchases, interest, fees, refunds and credits combined, not gross purchases.
        public decimal EstimatedNetActivity { get; init; }
        public decimal ClosingBalance { get; init; }
        // Closing - opening. Negative means the debt went down.
        public decimal NetDebtMovement { get; init; }
        public string DebtStatus { get; init; } = string.Empty;
        // The latest statement is the baseline of the open period that follows it.
        public bool IsLatest { get; init; }
        public string Notes { get; init; } = string.Empty;
    }

    // The period after the latest statement (or after the initial balance when no statement exists yet).
    // Its net activity is unknown until the next statement is recorded.
    public class OpenPeriod
    {
        public DateOnly SinceDate { get; init; }
        public Guid? LatestStatementId { get; init; }
        public decimal OpeningBalance { get; init; }
        public decimal PaymentsSoFar { get; init; }
        public int PaymentCount { get; init; }
        // Opening balance minus payments made since; new charges are not known yet.
        public decimal CurrentOutstanding { get; init; }
    }

    public class CreditCardLedger
    {
        public decimal InitialBalance { get; init; }
        public DateOnly InitialBalanceDate { get; init; }
        public IReadOnlyList<StatementPeriod> Periods { get; init; } = [];
        public OpenPeriod OpenPeriod { get; init; } = new();
        // Payments on or before the initial balance date are already part of the initial balance and are not counted.
        public IReadOnlyList<LedgerPayment> CountedPayments { get; init; } = [];
        public decimal CurrentOutstanding => OpenPeriod.CurrentOutstanding;
        public decimal LatestStatementBalance => OpenPeriod.OpeningBalance;
        // Positive = debt reduced since the initial balance.
        public decimal DebtReduction => InitialBalance - CurrentOutstanding;
        public decimal? DebtReductionPercent => CreditCardDebtCalculator.Percent(DebtReduction, InitialBalance);
    }

    // Statement-to-statement debt calculations. Every figure shown in the Credit Card Tracker comes from here,
    // so the overview, statement history, payments and charts always agree.
    public static class CreditCardDebtCalculator
    {
        public static CreditCardLedger Calculate(decimal initialBalance, DateOnly initialBalanceDate, IEnumerable<LedgerStatement> statements, IEnumerable<LedgerPayment> payments)
        {
            var orderedStatements = statements.OrderBy(x => x.PeriodEndDate).ToList();
            var countedPayments = payments
                .Where(x => x.PaymentDate > initialBalanceDate)
                .OrderBy(x => x.PaymentDate)
                .ToList();

            var periods = new List<StatementPeriod>();
            var cutoff = initialBalanceDate;
            var balance = initialBalance;

            for (var i = 0; i < orderedStatements.Count; i++)
            {
                var statement = orderedStatements[i];
                var periodPayments = PaymentsBetween(countedPayments, cutoff, statement.PeriodEndDate);
                var totalPayments = periodPayments.Sum(x => x.Amount);
                var movement = statement.ClosingBalance - balance;

                periods.Add(new StatementPeriod
                {
                    StatementId = statement.StatementId,
                    StatementDate = statement.StatementDate,
                    PeriodStartDate = statement.PeriodStartDate,
                    PeriodEndDate = statement.PeriodEndDate,
                    PreviousCutoffDate = cutoff,
                    OpensFromInitialBalance = i == 0,
                    OpeningBalance = balance,
                    TotalPayments = totalPayments,
                    PaymentCount = periodPayments.Count,
                    EstimatedNetActivity = EstimatedNetActivity(balance, statement.ClosingBalance, totalPayments),
                    ClosingBalance = statement.ClosingBalance,
                    NetDebtMovement = movement,
                    DebtStatus = DebtStatus.From(movement),
                    IsLatest = i == orderedStatements.Count - 1,
                    Notes = statement.Notes
                });

                cutoff = statement.PeriodEndDate;
                balance = statement.ClosingBalance;
            }

            var openPayments = countedPayments.Where(x => x.PaymentDate > cutoff).ToList();
            var paymentsSoFar = openPayments.Sum(x => x.Amount);

            return new CreditCardLedger
            {
                InitialBalance = initialBalance,
                InitialBalanceDate = initialBalanceDate,
                Periods = periods,
                CountedPayments = countedPayments,
                OpenPeriod = new OpenPeriod
                {
                    SinceDate = cutoff,
                    LatestStatementId = orderedStatements.LastOrDefault()?.StatementId,
                    OpeningBalance = balance,
                    PaymentsSoFar = paymentsSoFar,
                    PaymentCount = openPayments.Count,
                    CurrentOutstanding = balance - paymentsSoFar
                }
            };
        }

        public static CreditCardLedger Calculate(Domain.Entities.CreditCard card)
            => Calculate(
                card.InitialBalance,
                card.InitialBalanceDate,
                card.Statements.Select(ToLedger),
                card.Payments.Select(ToLedger));

        // Next statement closing - previous statement closing + payments made between the two statements.
        public static decimal EstimatedNetActivity(decimal previousClosingBalance, decimal closingBalance, decimal paymentsBetween)
            => closingBalance - previousClosingBalance + paymentsBetween;

        // The statement period a payment counts toward: the first statement whose cutoff is on or after the payment date.
        // Null means the open period (no statement covers it yet) or a payment on or before the initial balance date.
        public static LedgerStatement? FindStatementFor(DateOnly paymentDate, DateOnly initialBalanceDate, IEnumerable<LedgerStatement> statements)
            => paymentDate <= initialBalanceDate
                ? null
                : statements.OrderBy(x => x.PeriodEndDate).FirstOrDefault(x => x.PeriodEndDate >= paymentDate);

        // Keeps each stored OpeningBalance equal to the previous statement's closing balance (the initial balance for the
        // first). Call after any statement is added, changed or removed, or the card's initial balance changes.
        // Returns true when a value changed.
        public static bool SyncOpeningBalances(decimal initialBalance, IEnumerable<Domain.Entities.CreditCardStatement> statements)
        {
            var changed = false;
            var balance = initialBalance;
            foreach (var statement in statements.OrderBy(x => x.PeriodEndDate))
            {
                if (statement.OpeningBalance != balance)
                {
                    statement.OpeningBalance = balance;
                    changed = true;
                }
                balance = statement.ClosingBalance;
            }
            return changed;
        }

        public static decimal? Percent(decimal part, decimal whole)
            => whole > 0 ? Math.Round(part / whole * 100, 2, MidpointRounding.AwayFromZero) : null;

        public static LedgerStatement ToLedger(Domain.Entities.CreditCardStatement statement)
            => new(statement.CreditCardStatementId, statement.StatementDate, statement.PeriodStartDate, statement.PeriodEndDate, statement.ClosingBalance, statement.Notes);

        public static LedgerPayment ToLedger(Domain.Entities.CreditCardPayment payment)
            => new(payment.CreditCardPaymentId, payment.PaymentDate, payment.Amount);

        private static List<LedgerPayment> PaymentsBetween(IEnumerable<LedgerPayment> payments, DateOnly afterCutoff, DateOnly upToCutoff)
            => payments.Where(x => x.PaymentDate > afterCutoff && x.PaymentDate <= upToCutoff).ToList();
    }
}
