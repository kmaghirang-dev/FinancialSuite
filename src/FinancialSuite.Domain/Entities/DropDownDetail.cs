using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinancialSuite.Domain.Entities
{
    public class DropDownDetail
    {
        [Key]
        public Guid DropDownDetailId { get; set; }
        public Guid DropDownCategoryId { get; set; }
        [MaxLength(100)]
        public string Code { get; set; } = string.Empty;
        [MaxLength(100)]
        public string DisplayValue { get; set; } = string.Empty;
        [MaxLength(100)]
        public string CreatedBy { get; set; } = string.Empty;
        public DateTime DateCreated { get; set; }
        [MaxLength(100)]
        public string UpdatedBy { get; set; } = string.Empty;
        public DateTime DateUpdated { get; set; }
        public bool IsActive { get; set; }
        public DropDownCategory? DropDownCategory { get; set; }
        public List<MediaLibrary>? MediaLibraryCategory { get; set; }
        public List<MediaLibrary>? MediaLibraryStatus { get; set; }
    }
}
