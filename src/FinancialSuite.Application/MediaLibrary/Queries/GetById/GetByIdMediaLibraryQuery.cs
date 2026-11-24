using MediatR;
using FinancialSuite.Application.MediaLibrary.Dtos;

namespace FinancialSuite.Application.MediaLibrary.Queries.GetById
{
    public class GetByIdMediaLibraryQuery(Guid id) : IRequest<GetByIdMediaLibraryDto>
    {
        public Guid Id { get; } = id;
    }
}
