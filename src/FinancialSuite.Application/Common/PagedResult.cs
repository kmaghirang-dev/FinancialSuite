using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinancialSuite.Application.Common
{
    public class PagedResult<T>
    {
        public PagedResult(IEnumerable<T> items, int totalCount, int pageSize, int pageNumber) 
        {
            Items = items;
            TotalCount = totalCount;
            TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
            PageSize = pageSize;
            PageNumber = pageNumber;
            ItemsFrom = (pageNumber - 1) * pageSize + 1;
            ItemsTo = ItemsFrom + pageSize - 1;
        }

        public IEnumerable<T>? Items { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int ItemsFrom { get; set; }
        public int ItemsTo { get; set; }
        public int TotalCount { get; set; }
        public int TotalPages { get; set; }
    }
}
