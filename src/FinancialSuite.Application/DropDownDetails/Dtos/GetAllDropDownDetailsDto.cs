using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinancialSuite.Application.DropDownDetails.Dtos
{
    public class GetAllDropDownDetailsDto
    {
        public Guid DropDownDetailId { get; set; }
        public Guid DropDownCategoryId { get; set; }
        public string Code { get; set; } = string.Empty;
        public string DisplayValue { get; set; } = string.Empty;
        public string CreatedBy { get; set; } = string.Empty;
        public string DateCreated { get; set; } = string.Empty;
        public string UpdatedBy { get; set; } = string.Empty;
        public string DateUpdated { get; set; } = string.Empty;
        public bool IsActive { get; set; }
    }
}
