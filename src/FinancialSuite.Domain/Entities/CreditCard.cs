using System.ComponentModel.DataAnnotations;

namespace FinancialSuite.Domain.Entities
{
    // A credit card tracked statement to statement. InitialBalance on InitialBalanceDate is the baseline the first
    // recorded statement is compared against; every amount after it is derived by CreditCardDebtCalculator.
    public class CreditCard
    {
        [Key]
        public Guid CreditCardId { get; set; }
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;
        [MaxLength(100)]
        public string Nickname { get; set; } = string.Empty;
        public decimal? CreditLimit { get; set; }
        public decimal InitialBalance { get; set; }
        public DateOnly InitialBalanceDate { get; set; }
        public int? StatementCutoffDay { get; set; }
        public int? PaymentDueDay { get; set; }
        [MaxLength(1000)]
        public string Notes { get; set; } = string.Empty;
        [MaxLength(100)]
        public string CreatedBy { get; set; } = string.Empty;
        public DateTime DateCreated { get; set; }
        [MaxLength(100)]
        public string UpdatedBy { get; set; } = string.Empty;
        public DateTime DateUpdated { get; set; }
        // False = archived: kept with its history but left out of the combined overview.
        public bool IsActive { get; set; }
        public List<CreditCardStatement> Statements { get; set; } = new List<CreditCardStatement>();
        public List<CreditCardPayment> Payments { get; set; } = new List<CreditCardPayment>();
    }
}
