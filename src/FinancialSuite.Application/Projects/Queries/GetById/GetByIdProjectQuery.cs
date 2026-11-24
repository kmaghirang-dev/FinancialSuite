using MediatR;
using FinancialSuite.Application.Projects.Dtos;

namespace FinancialSuite.Application.Projects.Queries.GetById
{
    public class GetByIdProjectQuery(Guid id) : IRequest<GetByIdProjectDto>
    {
        public Guid Id { get; } = id;
    }
}
