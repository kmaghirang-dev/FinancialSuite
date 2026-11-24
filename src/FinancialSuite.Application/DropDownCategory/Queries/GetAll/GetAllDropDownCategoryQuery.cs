using MediatR;
using FinancialSuite.Application.DropDownCategory.Dtos;

namespace FinancialSuite.Application.DropDownCategory.Queries.GetAll
{
    public class GetAllDropDownCategoryQuery : IRequest<IEnumerable<GetAllDropDownCategoryDto>>
    {
    }
}
