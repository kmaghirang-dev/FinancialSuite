using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinancialSuite.Application.DropDownDetails.Dtos
{
    public class GetByIdDropDownDetailsDto
    {
        public Guid DropDownDetailId { get; set; }
        public string Code { get; set; } = string.Empty;
        public string DisplayValue { get; set; } = string.Empty;
        public string CreatedBy { get; set; } = string.Empty;
    }
}
