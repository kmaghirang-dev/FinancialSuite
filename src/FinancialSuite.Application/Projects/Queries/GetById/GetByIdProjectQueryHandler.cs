using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using FinancialSuite.Application.Projects.Dtos;
using FinancialSuite.Domain.Exceptions;
using FinancialSuite.Domain.Repositories;

namespace FinancialSuite.Application.Projects.Queries.GetById
{
    public class GetByIdProjectQueryHandler(ILogger<GetByIdProjectQueryHandler> logger, IMapper mapper, IProjectsRepository repository) : IRequestHandler<GetByIdProjectQuery, GetByIdProjectDto>
    {
        public async Task<GetByIdProjectDto> Handle(GetByIdProjectQuery request, CancellationToken cancellationToken)
        {
            var id = request.Id;
            var data = await repository.GetByIdAsync(id) ??
                       throw new NotFoundException(nameof(Projects), id.ToString());

            return mapper.Map<GetByIdProjectDto>(data);
        }
    }
}
