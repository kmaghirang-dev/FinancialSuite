using Microsoft.EntityFrameworkCore;
using FinancialSuite.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using FinancialSuite.Domain.Constants;
using FinancialSuite.Domain.Exceptions;
using FinancialSuite.Domain.Interfaces;

namespace FinancialSuite.Infrastructure.Persistence
{
    public class MyApplicationDbContext(DbContextOptions<MyApplicationDbContext> options, IDataOwnershipContext ownershipContext) : IdentityDbContext<User>(options)
    {
        private const string OwnershipProperty = "CreatedBy";

        // Read by the ownership query filters on every query.
        private bool IsSystemOperation => ownershipContext.IsSystemOperation;
        private string? CurrentUserEmail => ownershipContext.CurrentUserEmail;

        internal DbSet<DropDownDetail> DropDownDetails { get; set; }
        internal DbSet<DropDownCategory> DropDownCategories { get; set; }
        internal DbSet<Project> Projects { get; set; }
        internal DbSet<ErrorLog> ErrorLogs { get; set; }
        internal DbSet<MediaLibrary> MediaLibrary { get; set; }
        internal DbSet<CreditCard> CreditCards { get; set; }
        internal DbSet<CreditCardStatement> CreditCardStatements { get; set; }
        internal DbSet<CreditCardPayment> CreditCardPayments { get; set; }

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

            modelBuilder.Entity<CreditCardStatement>()
                .HasOne(x => x.CreditCard)
                .WithMany(x => x.Statements)
                .HasForeignKey(x => x.CreditCardId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<CreditCardPayment>()
                .HasOne(x => x.CreditCard)
                .WithMany(x => x.Payments)
                .HasForeignKey(x => x.CreditCardId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<CreditCard>().Property(x => x.CreditLimit).HasPrecision(18, 2);
            modelBuilder.Entity<CreditCard>().Property(x => x.InitialBalance).HasPrecision(18, 2);
            modelBuilder.Entity<CreditCardStatement>().Property(x => x.OpeningBalance).HasPrecision(18, 2);
            modelBuilder.Entity<CreditCardStatement>().Property(x => x.ClosingBalance).HasPrecision(18, 2);
            modelBuilder.Entity<CreditCardPayment>().Property(x => x.Amount).HasPrecision(18, 2);

            // One statement per cutoff date per card; overlapping periods are rejected by the statement handlers.
            modelBuilder.Entity<CreditCardStatement>()
                .HasIndex(x => new { x.CreditCardId, x.PeriodEndDate })
                .IsUnique();

            modelBuilder.Entity<CreditCardPayment>()
                .HasIndex(x => new { x.CreditCardId, x.PaymentDate });

            ApplyOwnershipFilter<DropDownCategory>(modelBuilder);
            ApplyOwnershipFilter<DropDownDetail>(modelBuilder);
            ApplyOwnershipFilter<MediaLibrary>(modelBuilder);
            ApplyOwnershipFilter<Project>(modelBuilder);
            ApplyOwnershipFilter<CreditCard>(modelBuilder);
            ApplyOwnershipFilter<CreditCardStatement>(modelBuilder);
            ApplyOwnershipFilter<CreditCardPayment>(modelBuilder);
        }

        public override int SaveChanges(bool acceptAllChangesOnSuccess)
        {
            ApplyOwnershipRules();
            return base.SaveChanges(acceptAllChangesOnSuccess);
        }

        public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
        {
            ApplyOwnershipRules();
            return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
        }

        // Users see their own records plus seeded/system records. Applies to every query, including Find and Include.
        private void ApplyOwnershipFilter<TEntity>(ModelBuilder modelBuilder) where TEntity : class
        {
            modelBuilder.Entity<TEntity>().HasQueryFilter(x =>
                IsSystemOperation ||
                EF.Property<string>(x, OwnershipProperty) == CurrentUserEmail ||
                DataOwnership.SystemCreators.Contains(EF.Property<string>(x, OwnershipProperty)));
        }

        // CreatedBy is always set here from the signed-in user, never from the request, and never changes afterwards.
        // Seeded/system records can only be changed by the owner account; anyone else's records not at all.
        private void ApplyOwnershipRules()
        {
            if (ownershipContext.IsSystemOperation) return;

            var currentUserEmail = ownershipContext.CurrentUserEmail;

            foreach (var entry in ChangeTracker.Entries())
            {
                if (entry.Metadata.FindProperty(OwnershipProperty) == null) continue;

                var createdBy = entry.Property(OwnershipProperty);

                if (entry.State == EntityState.Added)
                {
                    createdBy.CurrentValue = currentUserEmail ?? throw new ForbidException();
                    continue;
                }

                if (entry.State != EntityState.Modified && entry.State != EntityState.Deleted) continue;

                var owner = (string?)createdBy.OriginalValue ?? string.Empty;
                var allowed = DataOwnership.SystemCreators.Contains(owner)
                    ? ownershipContext.IsOwner
                    : string.Equals(owner, currentUserEmail, StringComparison.OrdinalIgnoreCase);

                if (!allowed) throw new ForbidException();

                createdBy.CurrentValue = owner;
                createdBy.IsModified = false;
            }
        }
    }
}
