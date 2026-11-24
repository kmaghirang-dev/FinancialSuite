using Microsoft.AspNetCore.Identity;
using FinancialSuite.Domain.Constants;
using FinancialSuite.Domain.Entities;
using FinancialSuite.Infrastructure.Persistence;
using FinancialSuite.Infrastructure.Seeders.RolesSeeder;

namespace FinancialSuite.Infrastructure.Seeders.UsersSeeder
{
    internal class UsersSeeder(MyApplicationDbContext dbContext, IPasswordHasher<User> passwordHasher) : IUsersSeeder
    {
        public async Task Seed()
        {
            if (await dbContext.Database.CanConnectAsync())
            {
                if (!dbContext.Users.Any())
                {
                    var users = new Dictionary<string, User>
                    {
                        { UserRoles.User, CreateUser("user@example.com", "user123") },
                        { UserRoles.Admin, CreateUser("admin@example.com", "admin123") },
                        { UserRoles.SuperAdmin, CreateUser("superadmin@example.com", "superadmin123") },
                    };

                    var roleNames = users.Keys.ToList();
                    var roles = dbContext.Roles
                        .Where(x => roleNames.Contains(x.Name!))
                        .ToList();

                    dbContext.Users.AddRange(users.Values);

                    foreach (var role in roles)
                    {
                        dbContext.UserRoles.Add(new IdentityUserRole<string>
                        {
                            UserId = users[role.Name!].Id,
                            RoleId = role.Id
                        });
                    }

                    await dbContext.SaveChangesAsync();
                }
            }
        }

        private User CreateUser(string email, string password)
        {
            var user = new User
            {
                UserName = email,
                NormalizedUserName = email.ToUpperInvariant(),
                Email = email,
                NormalizedEmail = email.ToUpperInvariant(),
                EmailConfirmed = true,
                SecurityStamp = Guid.NewGuid().ToString(),
                ConcurrencyStamp = Guid.NewGuid().ToString()
            };

            user.PasswordHash = passwordHasher.HashPassword(user, password);

            return user;
        }
    }
}
