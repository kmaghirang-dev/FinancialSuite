using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http.Json;
using System.Threading.Tasks;
using FluentAssertions;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Authorization.Policy;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Moq;
using FinancialSuite.API.Controllers;
using FinancialSuite.Application.DropDownCategory.Commands.Create;
using FinancialSuite.Application.DropDownCategory.Commands.Toggle;
using FinancialSuite.Application.DropDownCategory.Commands.Update;
using FinancialSuite.Application.DropDownCategory.Dtos;
using FinancialSuite.Domain.Entities;
using FinancialSuite.Domain.Repositories;
using Xunit;

namespace FinancialSuite.API.Tests.Controllers;

[TestSubject(typeof(DropDownCategoryController))]
public class DropDownCategoryControllerTest : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly Mock<IDropDownCategoryRepository> _repositoryMock = new();

    public DropDownCategoryControllerTest(WebApplicationFactory<Program> factory)
    {
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                services.Replace(ServiceDescriptor.Singleton<IPolicyEvaluator, FakePolicyEvaluator>());
                services.Replace(ServiceDescriptor.Scoped(typeof(IDropDownCategoryRepository), _ => _repositoryMock.Object));
            });
        });
    }

    [Fact]
    public async Task GetAll_ForValidRequest_Returns200Ok()
    {
        // Arrange
        _repositoryMock.Setup(repo => repo.GetAllAsync())
            .ReturnsAsync(new List<DropDownCategory> { new DropDownCategory { DropDownCategoryId = Guid.NewGuid(), IsActive = true } });
        var client = _factory.CreateClient();

        // Act
        var result = await client.GetAsync("api/dropdown/category", TestContext.Current.CancellationToken);
        var returned = await result.Content.ReadFromJsonAsync<List<GetAllDropDownCategoryDto>>(TestContext.Current.CancellationToken);

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.OK);
        returned.Should().ContainSingle();
    }

    [Fact]
    public async Task GetAllPaginated_ForValidRequest_Returns200Ok()
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act
        var result = await client.GetAsync("api/dropdown/category/paginated?pageNumber=1&pageSize=10", TestContext.Current.CancellationToken);

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetById_ForExistingId_Returns200Ok()
    {
        // Arrange
        var id = Guid.NewGuid();
        _repositoryMock.Setup(repo => repo.GetByIdAsync(id))
            .ReturnsAsync(new DropDownCategory { DropDownCategoryId = id, Value = "Test Value", DropDownDetails = [] });
        var client = _factory.CreateClient();

        // Act
        var result = await client.GetAsync($"api/dropdown/category/{id}", TestContext.Current.CancellationToken);
        var returned = await result.Content.ReadFromJsonAsync<GetByIdDropDownCategoryDto>(TestContext.Current.CancellationToken);

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.OK);
        returned.Should().NotBeNull();
        returned!.DropDownCategoryId.Should().Be(id);
        returned.Value.Should().Be("Test Value");
    }

    [Fact]
    public async Task Create_ForValidCommand_Returns201Created()
    {
        // Arrange
        var id = Guid.NewGuid();
        _repositoryMock.Setup(repo => repo.CreateAsync(It.IsAny<DropDownCategory>()))
            .ReturnsAsync(id);
        var client = _factory.CreateClient();

        var command = new CreateDropDownCategoryCommand { Value = "Test Value", CreatedBy = "Creator" };

        // Act
        var result = await client.PostAsJsonAsync("api/dropdown/category", command, TestContext.Current.CancellationToken);

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.Created);
        result.Headers.Location!.ToString().Should().EndWith($"/api/dropdown/category/{id}");
        _repositoryMock.Verify(repo => repo.CreateAsync(It.IsAny<DropDownCategory>()), Times.Once);
    }

    [Fact]
    public async Task Create_ForInvalidCommand_Returns400BadRequest()
    {
        // Arrange
        var client = _factory.CreateClient();

        var command = new CreateDropDownCategoryCommand { Value = "", CreatedBy = "" };

        // Act
        var result = await client.PostAsJsonAsync("api/dropdown/category", command, TestContext.Current.CancellationToken);

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        _repositoryMock.Verify(repo => repo.CreateAsync(It.IsAny<DropDownCategory>()), Times.Never);
    }

    [Fact]
    public async Task Update_ForExistingId_Returns201Created()
    {
        // Arrange
        var id = Guid.NewGuid();
        _repositoryMock.Setup(repo => repo.GetByIdAsync(id))
            .ReturnsAsync(new DropDownCategory { DropDownCategoryId = id });
        var client = _factory.CreateClient();

        var command = new UpdateDropDownCategoryCommand { Value = "Updated Value", UpdatedBy = "Updater" };

        // Act
        var result = await client.PatchAsJsonAsync($"api/dropdown/category/{id}", command, TestContext.Current.CancellationToken);

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.Created);
        result.Headers.Location!.ToString().Should().EndWith($"/api/dropdown/category/{id}");
        _repositoryMock.Verify(repo => repo.SaveChanges(), Times.Once);
    }

    [Fact]
    public async Task Update_ForNonExistingId_Returns404NotFound()
    {
        // Arrange
        var id = Guid.NewGuid();
        _repositoryMock.Setup(repo => repo.GetByIdAsync(id))
            .ReturnsAsync((DropDownCategory)null);
        var client = _factory.CreateClient();

        var command = new UpdateDropDownCategoryCommand { Value = "Updated Value", UpdatedBy = "Updater" };

        // Act
        var result = await client.PatchAsJsonAsync($"api/dropdown/category/{id}", command, TestContext.Current.CancellationToken);

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Update_ForInvalidCommand_Returns400BadRequest()
    {
        // Arrange
        var id = Guid.NewGuid();
        var client = _factory.CreateClient();

        var command = new UpdateDropDownCategoryCommand { Value = "", UpdatedBy = "" };

        // Act
        var result = await client.PatchAsJsonAsync($"api/dropdown/category/{id}", command, TestContext.Current.CancellationToken);

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        _repositoryMock.Verify(repo => repo.SaveChanges(), Times.Never);
    }

    [Fact]
    public async Task Toggle_ForExistingId_Returns204NoContent()
    {
        // Arrange
        var id = Guid.NewGuid();
        _repositoryMock.Setup(repo => repo.GetByIdAsync(id))
            .ReturnsAsync(new DropDownCategory { DropDownCategoryId = id, IsActive = true });
        var client = _factory.CreateClient();

        var command = new ToggleDropDownCategoryCommand { UpdatedBy = "Updater", IsActive = false };

        // Act
        var result = await client.PatchAsJsonAsync($"api/dropdown/category/toggle/{id}", command, TestContext.Current.CancellationToken);

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.NoContent);
        _repositoryMock.Verify(repo => repo.SaveChanges(), Times.Once);
    }

    [Fact]
    public async Task Toggle_ForNonExistingId_Returns404NotFound()
    {
        // Arrange
        var id = Guid.NewGuid();
        _repositoryMock.Setup(repo => repo.GetByIdAsync(id))
            .ReturnsAsync((DropDownCategory)null);
        var client = _factory.CreateClient();

        var command = new ToggleDropDownCategoryCommand { UpdatedBy = "Updater", IsActive = false };

        // Act
        var result = await client.PatchAsJsonAsync($"api/dropdown/category/toggle/{id}", command, TestContext.Current.CancellationToken);

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Delete_ForExistingId_Returns204NoContent()
    {
        // Arrange
        var id = Guid.NewGuid();
        var data = new DropDownCategory { DropDownCategoryId = id };
        _repositoryMock.Setup(repo => repo.GetByIdAsync(id))
            .ReturnsAsync(data);
        var client = _factory.CreateClient();

        // Act
        var result = await client.DeleteAsync($"api/dropdown/category/{id}", TestContext.Current.CancellationToken);

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.NoContent);
        _repositoryMock.Verify(repo => repo.DeleteAsync(data), Times.Once);
    }

    [Fact]
    public async Task Delete_ForNonExistingId_Returns404NotFound()
    {
        // Arrange
        var id = Guid.NewGuid();
        _repositoryMock.Setup(repo => repo.GetByIdAsync(id))
            .ReturnsAsync((DropDownCategory)null);
        var client = _factory.CreateClient();

        // Act
        var result = await client.DeleteAsync($"api/dropdown/category/{id}", TestContext.Current.CancellationToken);

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.NotFound);
        _repositoryMock.Verify(repo => repo.DeleteAsync(It.IsAny<DropDownCategory>()), Times.Never);
    }
}
