using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using FinancialSuite.Application.MediaLibrary.Dtos;
using FinancialSuite.Domain.Exceptions;
using FinancialSuite.Domain.Repositories;

namespace FinancialSuite.Application.MediaLibrary.Queries.GetById
{
    public class GetByIdMediaLibraryQueryHandler(ILogger<GetByIdMediaLibraryQueryHandler> logger, IMapper mapper, IMediaLibraryRepository repository) : IRequestHandler<GetByIdMediaLibraryQuery, GetByIdMediaLibraryDto>
    {
        public async Task<GetByIdMediaLibraryDto> Handle(GetByIdMediaLibraryQuery request, CancellationToken cancellationToken)
        {
            var id = request.Id;
            var data = await repository.GetByIdAsync(id) ??
                       throw new NotFoundException(nameof(MediaLibrary), id.ToString());

            return mapper.Map<GetByIdMediaLibraryDto>(data);
        }
    }
}
