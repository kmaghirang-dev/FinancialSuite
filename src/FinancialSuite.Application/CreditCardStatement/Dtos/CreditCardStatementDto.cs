namespace FinancialSuite.Application.CreditCardStatement.Dtos
{
    public class CreditCardStatementDto
    {
        public Guid CreditCardStatementId { get; set; }
        public Guid CreditCardId { get; set; }
        public string CreditCardName { get; set; } = string.Empty;
        public DateOnly StatementDate { get; set; }
        public DateOnly PeriodStartDate { get; set; }
        public DateOnly PeriodEndDate { get; set; }
        public decimal OpeningBalance { get; set; }
        public decimal ClosingBalance { get; set; }
        public string Notes { get; set; } = string.Empty;
        public string CreatedBy { get; set; } = string.Empty;
        public string DateCreated { get; set; } = string.Empty;
        public string UpdatedBy { get; set; } = string.Empty;
        public string DateUpdated { get; set; } = string.Empty;
    }

    // What saving a statement would produce, shown before it is saved.
    public class CreditCardStatementPreviewDto
    {
        public decimal PreviousBalance { get; set; }
        public DateOnly PreviousCutoffDate { get; set; }
        // True when the previous balance is the card's initial balance rather than a statement.
        public bool OpensFromInitialBalance { get; set; }
        public DateOnly? PreviousStatementDate { get; set; }
        public decimal PaymentsSincePrevious { get; set; }
        public int PaymentCount { get; set; }
        // Null until a closing balance is given.
        public decimal? NewBalance { get; set; }
        public decimal? EstimatedNetActivity { get; set; }
        public decimal? NetDebtMovement { get; set; }
        public string? DebtStatus { get; set; }
        // Statements after this one whose opening balance and activity will be recalculated.
        public int LaterStatementCount { get; set; }
        // Why the statement cannot be saved as entered (e.g. an overlapping period); null when it can.
        public string? Problem { get; set; }
    }
}
