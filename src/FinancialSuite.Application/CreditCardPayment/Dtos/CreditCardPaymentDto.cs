namespace FinancialSuite.Application.CreditCardPayment.Dtos
{
    public class CreditCardPaymentDto
    {
        public Guid CreditCardPaymentId { get; set; }
        public Guid CreditCardId { get; set; }
        public string CreditCardName { get; set; } = string.Empty;
        public DateOnly PaymentDate { get; set; }
        public decimal Amount { get; set; }
        public string PaymentReference { get; set; } = string.Empty;
        public string Notes { get; set; } = string.Empty;

        // The statement period the payment counts toward. Both null = the open period, pending the next statement.
        public Guid? StatementId { get; set; }
        public DateOnly? StatementPeriodEndDate { get; set; }
        // On or before the card's initial balance date: already part of that balance, so not counted.
        public bool IsBeforeInitialBalance { get; set; }

        public string CreatedBy { get; set; } = string.Empty;
        public string DateCreated { get; set; } = string.Empty;
        public string UpdatedBy { get; set; } = string.Empty;
        public string DateUpdated { get; set; } = string.Empty;
    }
}
