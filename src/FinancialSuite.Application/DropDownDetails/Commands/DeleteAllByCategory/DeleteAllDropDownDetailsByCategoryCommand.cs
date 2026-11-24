using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinancialSuite.Application.DropDownDetails.Commands.DeleteAllByCategory
{
    public class DeleteAllDropDownDetailsByCategoryCommand(Guid id) : IRequest
    {
        public Guid Id { get; set; } = id;
    }
}
