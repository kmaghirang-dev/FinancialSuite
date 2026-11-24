using FinancialSuite.Application.Common;
using FinancialSuite.Domain.Entities;

namespace FinancialSuite.Domain.Repositories
{
    public interface IMediaLibraryRepository : IBaseRepository<MediaLibrary>
    {
        Task<(IEnumerable<MediaLibrary>, int)> GetAllMatchingAsync(Guid? categoryId, string? searchPhrase, int pageSize, int pageNumber, string? sortBy, SortDirection? sortDirection);
    }
}
