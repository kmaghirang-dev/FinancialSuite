using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using FinancialSuite.Application.Projects.Dtos;
using FinancialSuite.Domain.Repositories;

namespace FinancialSuite.Application.Projects.Queries.GetAll
{
    public class GetAllProjectsQueryHandler(ILogger<GetAllProjectsQueryHandler> logger, IMapper mapper, IProjectsRepository repository) : IRequestHandler<GetAllProjectsQuery, IEnumerable<GetAllProjectsDto>>
    {
        public async Task<IEnumerable<GetAllProjectsDto>> Handle(GetAllProjectsQuery request, CancellationToken cancellationToken)
        {
            var data = await repository.GetAllAsync();
            data = data.Where(x => x.IsActive);
            return mapper.Map<IEnumerable<GetAllProjectsDto>>(data);
        }
    }
}
