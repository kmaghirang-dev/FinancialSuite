using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using FinancialSuite.Application.DropDownDetails.Dtos;

namespace FinancialSuite.Application.DropDownDetails.Queries.GetById
{
    public class GetByIdDropDownDetailsQuery(Guid id, Guid detailId) : IRequest<GetByIdDropDownDetailsDto>
    {
        public Guid Id { get; set; } = id;
        public Guid DetailId { get; set; } = detailId;
    }   
}
