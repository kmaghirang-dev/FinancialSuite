using MediatR;
using FinancialSuite.Application.MediaLibrary.Dtos;

namespace FinancialSuite.Application.MediaLibrary.Queries.GetAll
{
    public class GetAllMediaLibraryQuery : IRequest<IEnumerable<GetAllMediaLibraryDto>>
    {
    }
}
