using System.ComponentModel.DataAnnotations;

namespace FinancialSuite.Domain.Entities
{
    // A payment toward a card. It counts toward the statement period whose cutoff is the first on or after PaymentDate.
    public class CreditCardPayment
    {
        [Key]
        public Guid CreditCardPaymentId { get; set; }
        public Guid CreditCardId { get; set; }
        public DateOnly PaymentDate { get; set; }
        public decimal Amount { get; set; }
        [MaxLength(100)]
        public string PaymentReference { get; set; } = string.Empty;
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
