using MediatR;
using FinancialSuite.Application.DropDownDetails.Dtos;

namespace FinancialSuite.Application.DropDownDetails.Queries.GetAllByCategoryValue
{
    public class GetDropDownDetailsByCategoryValueQuery(string value) : IRequest<IEnumerable<GetAllDropDownDetailsDto>>
    {
        public string Value { get; } = value;
    }
}
