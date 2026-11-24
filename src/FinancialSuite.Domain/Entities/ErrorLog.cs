using System.ComponentModel.DataAnnotations;

namespace FinancialSuite.Domain.Entities
{
    public class ErrorLog
    {
        [Key]
        public Guid ErrorLogId { get; set; }
        public string Message { get; set; } = string.Empty;
        public string Exception { get; set; } = string.Empty;
        [MaxLength(10)]
        public string RequestMethod { get; set; } = string.Empty;
        public string RequestPath { get; set; } = string.Empty;
        public DateTime DateCreated { get; set; }
    }
}
