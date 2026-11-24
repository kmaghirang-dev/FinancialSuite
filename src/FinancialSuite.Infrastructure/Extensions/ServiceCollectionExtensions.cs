


using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using FinancialSuite.Domain.Entities;
using FinancialSuite.Domain.Interfaces;
using FinancialSuite.Domain.Interfaces.Utilities;
using FinancialSuite.Domain.Repositories;
using FinancialSuite.Infrastructure.Authorization;
using FinancialSuite.Infrastructure.Authorization.Requirements.MinimumAgeRequirement;
using FinancialSuite.Infrastructure.Authorization.Services;
using FinancialSuite.Infrastructure.Utilities.Api;
using FinancialSuite.Infrastructure.Utilities.Encryption;
using FinancialSuite.Infrastructure.Utilities.Excel;
using FinancialSuite.Infrastructure.Persistence;
using FinancialSuite.Infrastructure.Repositories;
using FinancialSuite.Infrastructure.Seeders.DropDownDetailsSeeder;
using FinancialSuite.Infrastructure.Seeders.ProjectsSeeder;
using FinancialSuite.Infrastructure.Seeders.RolesSeeder;
using FinancialSuite.Infrastructure.Seeders.UsersSeeder;
using FinancialSuite.Infrastructure.Utilities.Word;

namespace FinancialSuite.Infrastructure.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static void AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            // Ownership rules read by MyApplicationDbContext; scoped so it follows the current request's user.
            services.AddScoped<IDataOwnershipContext, DataOwnershipContext>();

            var connectionString = configuration.GetConnectionString("DefaultConnection");
            services.AddDbContext<MyApplicationDbContext>(options => 
                options.UseSqlServer(connectionString)
                    .EnableSensitiveDataLogging());

            services.AddIdentityApiEndpoints<User>()
                .AddRoles<IdentityRole>()
                .AddClaimsPrincipalFactory<FinancialSuiteClaimsPrincipalFactory>()
                .AddEntityFrameworkStores<MyApplicationDbContext>();

            // Seeders
            services.AddScoped<IDropDownDetailsSeeder, DropDownDetailsSeeder>();
            services.AddScoped<IRolesSeeder, RolesSeeder>();
            services.AddScoped<IUsersSeeder, UsersSeeder>();
            services.AddScoped<IProjectsSeeder, ProjectsSeeder>();

            // Repository registrations
            services.AddScoped<IDropDownCategoryRepository, DropDownCategoryRepository>();
            services.AddScoped<IDropDownDetailsRepository, DropDownDetailsRepository>();
            services.AddScoped<IProjectsRepository, ProjectsRepository>();
            services.AddScoped<IErrorLogRepository, ErrorLogRepository>();
            services.AddScoped<IMediaLibraryRepository, MediaLibraryRepository>();
            services.AddScoped<ICreditCardRepository, CreditCardRepository>();
            services.AddScoped<ICreditCardStatementRepository, CreditCardStatementRepository>();
            services.AddScoped<ICreditCardPaymentRepository, CreditCardPaymentRepository>();

            // External APIs — inject IApiConnector to call absolute URLs. An API with its own base address and timeout
            // registers a named HttpClient and wraps it in an ApiConnector, for example:
            //   services.AddHttpClient("MyApi", client => client.BaseAddress = new Uri(configuration["MyApi:BaseUrl"]!));
            //   services.AddTransient<IMyApiClient>(sp => new MyApiClient(
            //       new ApiConnector(sp.GetRequiredService<IHttpClientFactory>().CreateClient("MyApi"))));
            services.AddHttpClient<IApiConnector, ApiConnector>();

            services.AddAuthorizationBuilder()
                // THIS is only a sample of policy that you can add. You can modify or remove it as needed. The policy below requires the user to have a claim of "Nationality" with a value of either "Filipino" or "American".
                //.AddPolicy(PolicyNames.HasNationality, policy => policy.RequireClaim(AppClaimTypes.Nationality,
                //    "Filipino","American"))
                // This is a sample of custom policy that you can add. You can modify or remove it as needed. The policy below requires the user to have a claim of "DateOfBirth" which is used to determine if the user is at least 20 years old.
                .AddPolicy(PolicyNames.AtLeast20, policy => policy.AddRequirements(new MinimumAgeRequirement(20)));

            services.AddScoped<IAuthorizationHandler, MinimumAgeRequirementHandler>();
            services.AddScoped<IProjectAuthorizationService, ProjectAuthorizationService>();

            // Encryption — key is a base64 32-byte value, kept in user secrets (Encryption:Key)
            var encryptionKey = configuration["Encryption:Key"] ??
                                throw new InvalidOperationException("Encryption:Key is not configured.");
            services.AddSingleton<IEncryptionService>(new EncryptionService(Convert.FromBase64String(encryptionKey)));

            // Documents — processed in memory, nothing is written to disk
            services.AddSingleton<IExcelReaderService, ExcelReaderService>();
            services.AddSingleton<IWordDocumentService, WordDocumentService>();
        }
    }
}
