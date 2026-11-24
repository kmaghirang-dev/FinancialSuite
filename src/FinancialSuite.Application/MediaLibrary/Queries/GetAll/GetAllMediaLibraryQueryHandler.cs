using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using FinancialSuite.Application.MediaLibrary.Dtos;
using FinancialSuite.Domain.Repositories;

namespace FinancialSuite.Application.MediaLibrary.Queries.GetAll
{
    public class GetAllMediaLibraryQueryHandler(ILogger<GetAllMediaLibraryQueryHandler> logger, IMapper mapper, IMediaLibraryRepository repository) : IRequestHandler<GetAllMediaLibraryQuery, IEnumerable<GetAllMediaLibraryDto>>
    {
        public async Task<IEnumerable<GetAllMediaLibraryDto>> Handle(GetAllMediaLibraryQuery request, CancellationToken cancellationToken)
        {
            var data = await repository.GetAllAsync();
            data = data.Where(x => x.IsActive);
            return mapper.Map<IEnumerable<GetAllMediaLibraryDto>>(data);
        }
    }
}
