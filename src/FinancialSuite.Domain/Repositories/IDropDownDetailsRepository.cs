using FinancialSuite.Application.Common;
using FinancialSuite.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinancialSuite.Domain.Repositories
{
    public interface IDropDownDetailsRepository : IBaseRepository<DropDownDetail>
    {
        Task CreateRangeAsync(IEnumerable<DropDownDetail> entities);
        Task DeleteAsync(IEnumerable<DropDownDetail> entities);
        Task<(IEnumerable<DropDownDetail>, int)> GetAllMatchingAsync(Guid dropDownCategoryId, string? searchPhrase, int pageSize, int pageNumber, string? sortBy, SortDirection? sortDirection);
    }
}
