using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinancialSuite.Domain.Entities
{
    public class DropDownCategory
    {
        [Key]
        public Guid DropDownCategoryId { get; set; }
        [MaxLength(100)]
        public string Value { get; set; } = string.Empty;
        [MaxLength(100)]
        public string CreatedBy { get; set; } = string.Empty;
        public DateTime DateCreated { get; set; }
        [MaxLength(100)]
        public string UpdatedBy { get; set; } = string.Empty;
        public DateTime DateUpdated { get; set; }
        public bool IsActive { get; set; }
        public List<DropDownDetail>? DropDownDetails { get; set; }
    }
}
