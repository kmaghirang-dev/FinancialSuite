using Microsoft.EntityFrameworkCore;
using FinancialSuite.Domain.Constants;
using FinancialSuite.Domain.Entities;
using FinancialSuite.Infrastructure.Persistence;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinancialSuite.Infrastructure.Seeders.DropDownDetailsSeeder
{
    internal class DropDownDetailsSeeder(MyApplicationDbContext dbContext) : IDropDownDetailsSeeder
    {
        public async Task Seed()
        {
            try
            {
                if (await dbContext.Database.CanConnectAsync())
                {
                    var list = await dbContext.DropDownCategories
                        .Include(x => x.DropDownDetails)
                        .ToListAsync();

                    var mediaType = GetDataMediaType();
                    var status = GetDataMediaStatus();

                    AddDetails(list, mediaType);
                    AddDetails(list, status);

                    if (dbContext.ChangeTracker.HasChanges())
                        await dbContext.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void AddDetails(List<DropDownCategory> list, IEnumerable<DropDownDetail> details)
        {
            foreach (var detail in details)
            {
                var category = GetOrAddCategory(list, detail.DropDownCategory!);

                // The same description may exist in other categories, but only once within this one.
                if (category.DropDownDetails!.Any(x => string.Equals(x.DisplayValue, detail.DisplayValue, StringComparison.OrdinalIgnoreCase))) continue;

                detail.DropDownCategoryId = category.DropDownCategoryId;
                detail.DropDownCategory = category;
                category.DropDownDetails!.Add(detail);
                dbContext.DropDownDetails.Add(detail);
            }
        }

        private DropDownCategory GetOrAddCategory(List<DropDownCategory> list, DropDownCategory category)
        {
            var existing = list.FirstOrDefault(x => string.Equals(x.Value, category.Value, StringComparison.OrdinalIgnoreCase));

            if (existing == null)
            {
                existing = category;
                dbContext.DropDownCategories.Add(existing);
                list.Add(existing);
            }

            existing.DropDownDetails ??= new List<DropDownDetail>();
            return existing;
        }

        private IEnumerable<DropDownDetail> GetDataMediaType()
        {
            var dateTime = DateTime.Now;
            var creator = "Seeder";

            var mediaType = new DropDownCategory
            {
                Value = DropDownCategoryValues.MediaType,
                CreatedBy = creator,
                DateCreated = dateTime,
                UpdatedBy = "",
                DateUpdated = dateTime,
                IsActive = true
            };

            return new List<DropDownDetail>
            {
                new DropDownDetail
                {
                    Code = "ANIME_SERIES",
                    DisplayValue = "Anime Series",
                    CreatedBy = creator,
                    DateCreated = dateTime,
                    UpdatedBy = "",
                    DateUpdated = dateTime,
                    IsActive = true,
                    DropDownCategory = mediaType
                },
                new DropDownDetail
                {
                    Code = "ANIME_MOVIE",
                    DisplayValue = "Anime Movie",
                    CreatedBy = creator,
                    DateCreated = dateTime,
                    UpdatedBy = "",
                    DateUpdated = dateTime,
                    IsActive = true,
                    DropDownCategory = mediaType
                },
                new DropDownDetail
                {
                    Code = "MOVIE",
                    DisplayValue = "Movie",
                    CreatedBy = creator,
                    DateCreated = dateTime,
                    UpdatedBy = "",
                    DateUpdated = dateTime,
                    IsActive = true,
                    DropDownCategory = mediaType
                },
                new DropDownDetail
                {
                    Code = "SERIES",
                    DisplayValue = "Series",
                    CreatedBy = creator,
                    DateCreated = dateTime,
                    UpdatedBy = "",
                    DateUpdated = dateTime,
                    IsActive = true,
                    DropDownCategory = mediaType
                },
                new DropDownDetail
                {
                    Code = "KDRAMA",
                    DisplayValue = "Kdrama",
                    CreatedBy = creator,
                    DateCreated = dateTime,
                    UpdatedBy = "",
                    DateUpdated = dateTime,
                    IsActive = true,
                    DropDownCategory = mediaType
                },
            };
        }

        private IEnumerable<DropDownDetail> GetDataMediaStatus()
        {

            var dateTime = DateTime.Now;
            var creator = "Seeder";

            var mediaStatus = new DropDownCategory
            {
                Value = "MediaStatus",
                CreatedBy = creator,
                DateCreated = dateTime,
                UpdatedBy = "",
                DateUpdated = dateTime,
                IsActive = true
            };

            return new List<DropDownDetail>
            {
                new DropDownDetail
                {
                    Code = "DONE",
                    DisplayValue = "Done",
                    CreatedBy = creator,
                    DateCreated = dateTime,
                    UpdatedBy = "",
                    DateUpdated = dateTime,
                    IsActive = true,
                    DropDownCategory = mediaStatus

                },
                new DropDownDetail
                {
                    Code = "PENDING",
                    DisplayValue = "Pending",
                    CreatedBy = creator,
                    DateCreated = dateTime,
                    UpdatedBy = "",
                    DateUpdated = dateTime,
                    IsActive = true,
                    DropDownCategory = mediaStatus

                },
                new DropDownDetail
                {
                    Code = "WAITING",
                    DisplayValue = "Waiting for next season.",
                    CreatedBy = creator,
                    DateCreated = dateTime,
                    UpdatedBy = "",
                    DateUpdated = dateTime,
                    IsActive = true,
                    DropDownCategory = mediaStatus
                },
                new DropDownDetail
                {
                    Code = "ONGOING",
                    DisplayValue = "On-going",
                    CreatedBy = creator,
                    DateCreated = dateTime,
                    UpdatedBy = "",
                    DateUpdated = dateTime,
                    IsActive = true,
                    DropDownCategory = mediaStatus
                },
            };
        }
    }
}
