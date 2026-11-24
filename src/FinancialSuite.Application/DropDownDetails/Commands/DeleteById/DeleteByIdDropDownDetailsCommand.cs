using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinancialSuite.Application.DropDownDetails.Commands.DeleteById
{
    public class DeleteByIdDropDownDetailsCommand(Guid id, Guid detailId) : IRequest
    {
        public Guid CategoryId { get; set; } = id;
        public Guid DetailId { get; set; } = detailId;
    }
}
