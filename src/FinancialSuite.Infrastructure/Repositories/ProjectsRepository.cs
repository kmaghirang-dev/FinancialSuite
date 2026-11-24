using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using FinancialSuite.Application.Common;
using FinancialSuite.Domain.Entities;
using FinancialSuite.Domain.Repositories;
using FinancialSuite.Infrastructure.Persistence;

namespace FinancialSuite.Infrastructure.Repositories
{
    internal class ProjectsRepository(MyApplicationDbContext dbContext)
        : BaseRepository<Project>(dbContext), IProjectsRepository
    {
        public async Task<(IEnumerable<Project>, int)> GetAllMatchingAsync(string? searchPhrase, int pageSize, int pageNumber, string? sortBy, SortDirection? sortDirection)
        {
            var searchPhraseLower = searchPhrase?.ToLower();

            // Initialize query to filter active projects and apply search phrase if provided
            var baseQuery = DbContext.Projects.Where(x =>
                    x.IsActive == true &&
                    (searchPhraseLower == null ||
                     (x.Name.ToLower().Contains(searchPhraseLower) ||
                      (x.Description != null && x.Description.ToLower().Contains(searchPhraseLower)))
                    )
                )
                .OrderByDescending(x => x.DateCreated);


            // Get the total count of matching projects before applying pagination
            var totalCount = await baseQuery.CountAsync();

            // Apply sorting if sortBy is provided
            if (sortBy != null)
            {
                var columnSelector = new Dictionary<string, Expression<Func<Project, object>>>
                {
                    { nameof(Project.Name), r => r.Name},
                    { nameof(Project.Description), r => r.Description ?? ""}
                };

                var selectedColumn = columnSelector[sortBy];

                baseQuery = sortDirection == SortDirection.Ascending
                    ? baseQuery.OrderBy(selectedColumn)
                    : baseQuery.OrderByDescending(selectedColumn);
            }

            // Execute the query with pagination
            var data = await baseQuery
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (data, totalCount);
        }
    }
}
