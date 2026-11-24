using Microsoft.EntityFrameworkCore;
using FinancialSuite.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace FinancialSuite.Infrastructure.Persistence
{
    public class MyApplicationDbContext(DbContextOptions<MyApplicationDbContext> options) : IdentityDbContext<User>(options)
    {
        internal DbSet<DropDownDetail> DropDownDetails { get; set; }
        internal DbSet<DropDownCategory> DropDownCategories { get; set; }
        internal DbSet<Project> Projects { get; set; }
        internal DbSet<ErrorLog> ErrorLogs { get; set; }
        internal DbSet<MediaLibrary> MediaLibrary { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<DropDownDetail>()
                .HasOne(x => x.DropDownCategory)
                .WithMany(x => x.DropDownDetails)
                .HasForeignKey(x => x.DropDownCategoryId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<MediaLibrary>()
                .HasOne(x => x.Category)
                .WithMany(x => x.MediaLibraryCategory)
                .HasForeignKey(x => x.CategoryId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<MediaLibrary>()
                .HasOne(x => x.Status)
                .WithMany(x => x.MediaLibraryStatus)
                .HasForeignKey(x => x.StatusId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Project>()
                .HasOne(x => x.Owner)
                .WithMany(x => x.OwnedProjects)
                .HasForeignKey(x => x.OwnerId)
                .OnDelete(DeleteBehavior.NoAction);
        }
    }
}
