using MediatR;
using FinancialSuite.Application.DropDownCategory.Dtos;

namespace FinancialSuite.Application.DropDownCategory.Queries.GetById
{
    public class GetByIdDropDownCategoryQuery(Guid id) : IRequest<GetByIdDropDownCategoryDto?>
    {
        public Guid Id { get; } = id;   
    }
}
