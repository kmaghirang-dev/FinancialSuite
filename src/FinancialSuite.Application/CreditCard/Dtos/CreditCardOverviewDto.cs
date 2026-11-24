using FinancialSuite.Application.CreditCard.Calculations;

namespace FinancialSuite.Application.CreditCard.Dtos
{
    public class CreditCardOverviewDto
    {
        public DateOnly? From { get; set; }
        public DateOnly? To { get; set; }
        public CreditCardSummaryDto Summary { get; set; } = new();
        public List<CreditCardLedgerDto> Cards { get; set; } = [];
        public List<CreditCardBalancePointDto> BalanceTrend { get; set; } = [];
        // Completed statement periods grouped by the month of their cutoff.
        public List<CreditCardPeriodTotalDto> PeriodTotals { get; set; } = [];
    }

    public class CreditCardSummaryDto
    {
        public int CardCount { get; set; }
        // Latest statement balances minus payments made since, across the cards shown. Not limited by the date range.
        public decimal TotalOutstanding { get; set; }
        // Payments dated within the range.
        public decimal TotalPayments { get; set; }
        public int PaymentCount { get; set; }
        // Completed statement periods whose cutoff falls within the range.
        public decimal EstimatedNetActivity { get; set; }
        public int CompletedPeriodCount { get; set; }
        public decimal NetDebtMovement { get; set; }
        public string DebtStatus { get; set; } = string.Empty;
        // Overall progress since each card's initial balance. Not limited by the date range.
        public decimal InitialTotalBalance { get; set; }
        public decimal DebtReduction { get; set; }
        public decimal? DebtReductionPercent { get; set; }
    }

    public class CreditCardLedgerDto
    {
        public Guid CreditCardId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Nickname { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public decimal? CreditLimit { get; set; }
        public decimal? AvailableCredit { get; set; }
        public bool IsActive { get; set; }
        public decimal InitialBalance { get; set; }
        public DateOnly InitialBalanceDate { get; set; }
        public decimal CurrentOutstanding { get; set; }
        public decimal LatestStatementBalance { get; set; }
        public DateOnly? LatestStatementDate { get; set; }
        public int StatementCount { get; set; }
        public decimal DebtReduction { get; set; }
        public decimal? DebtReductionPercent { get; set; }
        // Oldest first.
        public IReadOnlyList<StatementPeriod> Periods { get; set; } = [];
        public OpenPeriod? OpenPeriod { get; set; }
    }

    public class CreditCardBalancePointDto
    {
        // "yyyy-MM"
        public string Month { get; set; } = string.Empty;
        public DateOnly Date { get; set; }
        public decimal Balance { get; set; }
        // True for today's outstanding balance; other points are month-end statement balances.
        public bool IsCurrent { get; set; }
    }

    public class CreditCardPeriodTotalDto
    {
        // "yyyy-MM"
        public string Month { get; set; } = string.Empty;
        public int PeriodCount { get; set; }
        public decimal TotalPayments { get; set; }
        public decimal EstimatedNetActivity { get; set; }
        public decimal NetDebtMovement { get; set; }
    }
}
