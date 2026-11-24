using Microsoft.AspNetCore.Identity;
using FinancialSuite.Domain.Constants;
using FinancialSuite.Domain.Entities;
using FinancialSuite.Infrastructure.Persistence;

namespace FinancialSuite.Infrastructure.Seeders.RolesSeeder
{
    internal class RolesSeeder(MyApplicationDbContext dbContext) : IRolesSeeder
    {
        public async Task Seed()
        {
            if (await dbContext.Database.CanConnectAsync())
            {
                if (!dbContext.Roles.Any())
                {
                    var roles = GetRoles();

                    dbContext.Roles.AddRange(roles);
                    await dbContext.SaveChangesAsync();
                }
            }
        }

        private IEnumerable<IdentityRole> GetRoles()
        {
            return new List<IdentityRole>
            {
                new IdentityRole(UserRoles.User)
                {
                    NormalizedName = UserRoles.User.ToUpperInvariant(),
                    Name = UserRoles.User,
                    ConcurrencyStamp = Guid.NewGuid().ToString()
                },
                new IdentityRole(UserRoles.Admin)
                {
                    NormalizedName = UserRoles.Admin.ToUpperInvariant(),
                    Name = UserRoles.Admin,
                    ConcurrencyStamp = Guid.NewGuid().ToString()
                },
                new IdentityRole(UserRoles.SuperAdmin)
                {
                    NormalizedName = UserRoles.SuperAdmin.ToUpperInvariant(),
                    Name = UserRoles.SuperAdmin,
                    ConcurrencyStamp = Guid.NewGuid().ToString()
                },
            };
        }
    }
}
