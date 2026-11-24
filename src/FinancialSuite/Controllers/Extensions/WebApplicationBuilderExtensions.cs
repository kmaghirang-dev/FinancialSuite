using Microsoft.OpenApi;
using FinancialSuite.API.Middlewares;
using Serilog;

namespace FinancialSuite.API.Controllers.Extensions
{
    public static class WebApplicationBuilderExtensions
    {
        public static void AddPresentation(this WebApplicationBuilder builder)
        {
            builder.Services.AddControllers();
            builder.Services.AddAuthentication();
            builder.Services.AddSwaggerGen(c =>
             {
                 c.AddSecurityDefinition("bearerAuth", new OpenApiSecurityScheme()
                 {
                     Type = SecuritySchemeType.Http,
                     Scheme = "Bearer",
                 });
                 c.AddSecurityRequirement(document => new OpenApiSecurityRequirement()
                 {
                     [new OpenApiSecuritySchemeReference("bearerAuth", document)] = []
                 });
             });

            builder.Services.AddEndpointsApiExplorer();

            builder.Services.AddScoped<ErrorHandlingMiddleware>();
            builder.Services.AddScoped<RequestTimeLoggingMiddleware>();

            builder.Host.UseSerilog((context, configuration) =>
                configuration.ReadFrom.Configuration(context.Configuration)
            );

        }
    }
}
