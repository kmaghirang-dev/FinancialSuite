using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinancialSuite.Application.DropDownCategory.Commands.Toggle
{
    public class ToggleDropDownCategoryCommand : IRequest
    {
        public Guid DropDownCategoryId { get; set; }
        public Guid DropDownDetailId { get; set; }
        public string UpdatedBy { get; set; } = string.Empty;
        public bool IsActive { get; set; }
    }
}
