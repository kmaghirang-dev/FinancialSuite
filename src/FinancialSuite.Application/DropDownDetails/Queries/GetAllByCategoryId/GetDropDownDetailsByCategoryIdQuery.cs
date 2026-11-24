using MediatR;
using FinancialSuite.Application.DropDownDetails.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinancialSuite.Application.DropDownDetails.Queries.GetAllByCategoryId
{
    public class GetDropDownDetailsByCategoryIdQuery(Guid id) : IRequest<IEnumerable<GetAllDropDownDetailsDto>>

    {
        public Guid Id { get; set; } = id;
    }
}
