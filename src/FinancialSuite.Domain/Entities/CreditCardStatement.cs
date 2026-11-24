using System.ComponentModel.DataAnnotations;

namespace FinancialSuite.Domain.Entities
{
    // One statement of account (SOA). Its closing balance covers activity up to PeriodEndDate (the cutoff).
    public class CreditCardStatement
    {
        [Key]
        public Guid CreditCardStatementId { get; set; }
        public Guid CreditCardId { get; set; }
        public DateOnly StatementDate { get; set; }
        public DateOnly PeriodStartDate { get; set; }
        public DateOnly PeriodEndDate { get; set; }
        // Always the previous statement's closing balance (or the card's initial balance); kept in sync on every change.
        public decimal OpeningBalance { get; set; }
        public decimal ClosingBalance { get; set; }
        [MaxLength(1000)]
        public string Notes { get; set; } = string.Empty;
        [MaxLength(100)]
        public string CreatedBy { get; set; } = string.Empty;
        public DateTime DateCreated { get; set; }
        [MaxLength(100)]
        public string UpdatedBy { get; set; } = string.Empty;
        public DateTime DateUpdated { get; set; }
        public CreditCard? CreditCard { get; set; }
    }
}
