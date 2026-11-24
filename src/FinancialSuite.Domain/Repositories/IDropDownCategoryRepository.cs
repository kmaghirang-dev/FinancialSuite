using FinancialSuite.Application.Common;
using FinancialSuite.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinancialSuite.Domain.Repositories
{
    public interface IDropDownCategoryRepository : IBaseRepository<DropDownCategory>
    {
        Task<DropDownCategory?> GetByValueAsync(string value);
        Task<(IEnumerable<DropDownCategory>, int)> GetAllMatchingAsync(string? searchPhrase, int pageSize, int pageNumber, string? sortBy, SortDirection? sortDirection);
    }
}
