using FinancialSuite.Application.CreditCard.Calculations;

namespace FinancialSuite.Application.CreditCard.Dtos
{
    public class CreditCardDto
    {
        public Guid CreditCardId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Nickname { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public decimal? CreditLimit { get; set; }
        public decimal InitialBalance { get; set; }
        public DateOnly InitialBalanceDate { get; set; }
        public int? StatementCutoffDay { get; set; }
        public int? PaymentDueDay { get; set; }
        public string Notes { get; set; } = string.Empty;

        // Calculated from the card's statements and payments.
        public decimal CurrentOutstanding { get; set; }
        public decimal? AvailableCredit { get; set; }
        public decimal LatestStatementBalance { get; set; }
        public DateOnly? LatestStatementDate { get; set; }
        // Cutoff of the latest statement, or the initial balance date: payments on or before InitialBalanceDate are not counted,
        // and a new statement normally ends after this date.
        public DateOnly LatestCutoffDate { get; set; }
        public int StatementCount { get; set; }
        public int PaymentCount { get; set; }
        public decimal DebtReduction { get; set; }
        public decimal? DebtReductionPercent { get; set; }

        public string CreatedBy { get; set; } = string.Empty;
        public string DateCreated { get; set; } = string.Empty;
        public string UpdatedBy { get; set; } = string.Empty;
        public string DateUpdated { get; set; } = string.Empty;
        public bool IsActive { get; set; }

        public CreditCardDto WithLedger(Domain.Entities.CreditCard card)
        {
            var ledger = CreditCardDebtCalculator.Calculate(card);
            DisplayName = CreditCardOverviewCalculator.DisplayName(card);
            CurrentOutstanding = ledger.CurrentOutstanding;
            AvailableCredit = card.CreditLimit - ledger.CurrentOutstanding;
            LatestStatementBalance = ledger.LatestStatementBalance;
            LatestStatementDate = ledger.Periods.LastOrDefault()?.StatementDate;
            LatestCutoffDate = ledger.OpenPeriod.SinceDate;
            StatementCount = ledger.Periods.Count;
            PaymentCount = card.Payments.Count;
            DebtReduction = ledger.DebtReduction;
            DebtReductionPercent = ledger.DebtReductionPercent;
            return this;
        }
    }
}
