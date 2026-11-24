using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using FinancialSuite.Application.Common;
using FinancialSuite.Domain.Entities;
using FinancialSuite.Domain.Repositories;
using FinancialSuite.Infrastructure.Persistence;

namespace FinancialSuite.Infrastructure.Repositories
{
    internal class MediaLibraryRepository(MyApplicationDbContext dbContext)
        : BaseRepository<MediaLibrary>(dbContext), IMediaLibraryRepository
    {
        public async Task<(IEnumerable<MediaLibrary>, int)> GetAllMatchingAsync(Guid? categoryId, string? searchPhrase, int pageSize, int pageNumber, string? sortBy, SortDirection? sortDirection)
        {
            var searchPhraseLower = searchPhrase?.ToLower();

            var baseQuery = DbContext.MediaLibrary.Where(x =>
                    x.IsActive == true &&
                    (categoryId == null || x.CategoryId == categoryId) &&
                    (searchPhraseLower == null ||
                     (x.Title.ToLower().Contains(searchPhraseLower) ||
                      x.Details.ToLower().Contains(searchPhraseLower))
                    )
                )
                .OrderByDescending(x => x.DateCreated);

            var totalCount = await baseQuery.CountAsync();

            if (sortBy != null)
            {
                var columnSelector = new Dictionary<string, Expression<Func<MediaLibrary, object>>>
                {
                    { nameof(MediaLibrary.Title), r => r.Title },
                    { nameof(MediaLibrary.Season), r => r.Season },
                    { nameof(MediaLibrary.EpisodeNo), r => r.EpisodeNo }
                };

                var selectedColumn = columnSelector[sortBy];

                baseQuery = sortDirection == SortDirection.Ascending
                    ? baseQuery.OrderBy(selectedColumn)
                    : baseQuery.OrderByDescending(selectedColumn);
            }

            var data = await baseQuery
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (data, totalCount);
        }
    }
}
