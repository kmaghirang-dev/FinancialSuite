using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;

namespace FinancialSuite.Application.DropDownCategory.Commands.Update
{
    public class UpdateDropDownCategoryCommand : IRequest
    {
        public Guid DropDownCategoryId { get; set; }
        public string Value { get; set; } = string.Empty;
        public string UpdatedBy { get; set; } = string.Empty;
    }
}
