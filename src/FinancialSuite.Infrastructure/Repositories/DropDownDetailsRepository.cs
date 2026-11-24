
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using FinancialSuite.Application.Common;
using FinancialSuite.Domain.Entities;
using FinancialSuite.Domain.Repositories;
using FinancialSuite.Infrastructure.Persistence;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinancialSuite.Infrastructure.Repositories
{
    public class DropDownDetailsRepository(MyApplicationDbContext dbContext)
        : BaseRepository<DropDownDetail>(dbContext), IDropDownDetailsRepository
    {
        public async Task CreateRangeAsync(IEnumerable<DropDownDetail> entities)
        {
            DbContext.DropDownDetails.AddRange(entities);
            await DbContext.SaveChangesAsync();
        }

        public async Task DeleteAsync(IEnumerable<DropDownDetail> entities)
        {
            DbContext.DropDownDetails.RemoveRange(entities);
            await DbContext.SaveChangesAsync();
        }

        public async Task<(IEnumerable<DropDownDetail>, int)> GetAllMatchingAsync(Guid dropDownCategoryId, string? searchPhrase, int pageSize, int pageNumber, string? sortBy, SortDirection? sortDirection)
        {
            var searchPhraseLower = searchPhrase?.ToLower();

            var baseQuery = DbContext.DropDownDetails.Where(x =>
                    x.DropDownCategoryId == dropDownCategoryId &&
                    x.IsActive == true &&
                    (searchPhraseLower == null ||
                     (x.Code.ToLower().Contains(searchPhraseLower) ||
                      x.DisplayValue.ToLower().Contains(searchPhraseLower))
                    )
                )
                .OrderByDescending(x => x.DateCreated);

            var totalCount = await baseQuery.CountAsync();

            if (sortBy != null)
            {
                var columnSelector = new Dictionary<string, Expression<Func<DropDownDetail, object>>>
                {
                    { nameof(DropDownDetail.Code), r => r.Code },
                    { nameof(DropDownDetail.DisplayValue), r => r.DisplayValue }
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
