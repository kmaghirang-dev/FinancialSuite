using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinancialSuite.Application.DropDownDetails.Commands.Create
{
    public class CreateDropDownDetailsCommand : IRequest<Guid>
    {
        public Guid DropDownCategoryId { get; set; }
        public string Code { get; set; } = string.Empty;
        public string DisplayValue { get; set; } = string.Empty;
        public string CreatedBy { get; set; } = string.Empty;
    }
}
