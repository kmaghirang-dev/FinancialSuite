using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using FinancialSuite.Application.Common;
using FinancialSuite.Domain.Entities;
using FinancialSuite.Domain.Repositories;
using FinancialSuite.Infrastructure.Persistence;

namespace FinancialSuite.Infrastructure.Repositories
{
    internal class DropDownCategoryRepository(MyApplicationDbContext dbContext)
        : BaseRepository<DropDownCategory>(dbContext), IDropDownCategoryRepository
    {
        public override async Task<DropDownCategory?> GetByIdAsync(Guid id)
        {
            var data = await DbContext.DropDownCategories
                .Include(x => x.DropDownDetails)
                .FirstOrDefaultAsync(x => x.DropDownCategoryId == id);
            return data;
        }

        public async Task<DropDownCategory?> GetByValueAsync(string value)
        {
            var data = await DbContext.DropDownCategories
                .Include(x => x.DropDownDetails)
                .FirstOrDefaultAsync(x => x.Value == value);
            return data;
        }

        public async Task<(IEnumerable<DropDownCategory>, int)> GetAllMatchingAsync(string? searchPhrase, int pageSize, int pageNumber, string? sortBy, SortDirection? sortDirection)
        {
            var searchPhraseLower = searchPhrase?.ToLower();

            var baseQuery = DbContext.DropDownCategories.Where(x =>
                    x.IsActive == true &&
                    (searchPhraseLower == null || x.Value.ToLower().Contains(searchPhraseLower))
                )
                .OrderByDescending(x => x.DateCreated);

            var totalCount = await baseQuery.CountAsync();

            if (sortBy != null)
            {
                var columnSelector = new Dictionary<string, Expression<Func<DropDownCategory, object>>>
                {
                    { nameof(DropDownCategory.Value), r => r.Value }
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
