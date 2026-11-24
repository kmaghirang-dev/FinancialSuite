using MediatR;
using FinancialSuite.Application.Projects.Dtos;

namespace FinancialSuite.Application.Projects.Queries.GetAll
{
    public class GetAllProjectsQuery : IRequest<IEnumerable<GetAllProjectsDto>>
    {
    }
}
