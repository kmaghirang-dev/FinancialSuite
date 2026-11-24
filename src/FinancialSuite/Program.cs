using Microsoft.OpenApi;
using FinancialSuite.API.Controllers.Extensions;
using FinancialSuite.API.Middlewares;
using FinancialSuite.Application.Extensions;
using FinancialSuite.Domain.Entities;
using FinancialSuite.Infrastructure.Extensions;
using FinancialSuite.Infrastructure.Seeders.DropDownDetailsSeeder;
using FinancialSuite.Infrastructure.Seeders.ProjectsSeeder;
using FinancialSuite.Infrastructure.Seeders.RolesSeeder;
using FinancialSuite.Infrastructure.Seeders.UsersSeeder;
using Serilog;
using Serilog.Events;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.AddPresentation();



builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var seeder = scope.ServiceProvider.GetRequiredService<IDropDownDetailsSeeder>();
    await seeder.Seed();

    var rolesSeeder = scope.ServiceProvider.GetRequiredService<IRolesSeeder>();
    await rolesSeeder.Seed();

    var usersSeeder = scope.ServiceProvider.GetRequiredService<IUsersSeeder>();
    await usersSeeder.Seed();

    var projectsSeeder = scope.ServiceProvider.GetRequiredService<IProjectsSeeder>();
    await projectsSeeder.Seed();
}

app.UseMiddleware<ErrorHandlingMiddleware>();
app.UseMiddleware<RequestTimeLoggingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Configure the HTTP request pipeline.
app.UseSerilogRequestLogging();

app.UseHttpsRedirection();

app.MapGroup("api/identity")
    .WithTags("Identity")
    .MapIdentityApi<User>();

app.UseAuthorization();

app.MapControllers();

app.Run();


public partial class Program {}