using System.Net;
using System.Threading.Tasks;
using FluentAssertions;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Moq;
using FinancialSuite.API.Controllers;
using FinancialSuite.Domain.Entities;
using FinancialSuite.Domain.Repositories;
using Xunit;

namespace FinancialSuite.API.Tests.Controllers;

[TestSubject(typeof(ErrorTestController))]
public class ErrorTestControllerTest : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly Mock<IErrorLogRepository> _errorLogRepositoryMock = new();

    public ErrorTestControllerTest(WebApplicationFactory<Program> factory)
    {
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                services.Replace(ServiceDescriptor.Scoped(typeof(IErrorLogRepository), _ => _errorLogRepositoryMock.Object));
            });
        });
    }

    [Fact]
    public async Task Throw_WhenCalled_Returns500AndLogsError()
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act
        var result = await client.GetAsync("api/error-test", TestContext.Current.CancellationToken);

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
        _errorLogRepositoryMock.Verify(repo => repo.CreateAsync(
            It.Is<ErrorLog>(log => log.RequestMethod == "GET" && log.RequestPath == "/api/error-test")), Times.Once);
    }
}
