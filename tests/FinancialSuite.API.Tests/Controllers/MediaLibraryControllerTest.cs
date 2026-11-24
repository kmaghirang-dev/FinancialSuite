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
using FinancialSuite.Application.MediaLibrary.Commands.Create;
using FinancialSuite.Application.MediaLibrary.Commands.Toggle;
using FinancialSuite.Application.MediaLibrary.Commands.Update;
using FinancialSuite.Application.MediaLibrary.Dtos;
using FinancialSuite.Application.Users;
using FinancialSuite.Domain.Constants;
using FinancialSuite.Domain.Repositories;
using Xunit;

namespace FinancialSuite.API.Tests.Controllers;

[TestSubject(typeof(MediaLibraryController))]
public class MediaLibraryControllerTest : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly Mock<IMediaLibraryRepository> _repositoryMock = new();
    private readonly Mock<IUserContext> _userContextMock = new();

    public MediaLibraryControllerTest(WebApplicationFactory<Program> factory)
    {
        var currentUser = new CurrentUser("1", "test@example.com", [UserRoles.SuperAdmin], null);
        _userContextMock.Setup(uc => uc.GetCurrentUser()).Returns(currentUser);

        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                services.Replace(ServiceDescriptor.Singleton<IPolicyEvaluator, FakePolicyEvaluator>());
                services.Replace(ServiceDescriptor.Scoped(typeof(IMediaLibraryRepository), _ => _repositoryMock.Object));
                services.Replace(ServiceDescriptor.Scoped(typeof(IUserContext), _ => _userContextMock.Object));
            });
        });
    }

    [Fact]
    public async Task GetAll_ForValidRequest_Returns200Ok()
    {
        // Arrange
        _repositoryMock.Setup(repo => repo.GetAllAsync())
            .ReturnsAsync(new List<Domain.Entities.MediaLibrary> { new Domain.Entities.MediaLibrary { MediaId = Guid.NewGuid(), IsActive = true } });
        var client = _factory.CreateClient();

        // Act
        var result = await client.GetAsync("api/media-library", TestContext.Current.CancellationToken);
        var returned = await result.Content.ReadFromJsonAsync<List<GetAllMediaLibraryDto>>(TestContext.Current.CancellationToken);

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
        var result = await client.GetAsync("api/media-library/paginated?pageNumber=1&pageSize=10", TestContext.Current.CancellationToken);

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetAllPaginated_WithCategoryId_FiltersByCategory()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        _repositoryMock.Setup(repo => repo.GetAllMatchingAsync(categoryId, null, 10, 1, null, It.IsAny<Application.Common.SortDirection?>()))
            .ReturnsAsync((new List<Domain.Entities.MediaLibrary>(), 0));

        var client = _factory.CreateClient();

        // Act
        var result = await client.GetAsync($"api/media-library/paginated?pageNumber=1&pageSize=10&categoryId={categoryId}", TestContext.Current.CancellationToken);

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.OK);
        _repositoryMock.Verify(repo => repo.GetAllMatchingAsync(categoryId, null, 10, 1, null, It.IsAny<Application.Common.SortDirection?>()), Times.Once);
    }

    [Fact]
    public async Task GetAllPaginated_ForInvalidRequest_Returns400BadRequest()
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act
        var result = await client.GetAsync("api/media-library/paginated", TestContext.Current.CancellationToken);

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetById_ForExistingId_Returns200Ok()
    {
        // Arrange
        var id = Guid.NewGuid();
        _repositoryMock.Setup(repo => repo.GetByIdAsync(id))
            .ReturnsAsync(new Domain.Entities.MediaLibrary { MediaId = id });

        var client = _factory.CreateClient();

        // Act
        var result = await client.GetAsync($"api/media-library/{id}", TestContext.Current.CancellationToken);
        var returned = await result.Content.ReadFromJsonAsync<GetByIdMediaLibraryDto>(TestContext.Current.CancellationToken);

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.OK);
        returned.Should().NotBeNull();
        returned!.MediaId.Should().Be(id);
    }

    [Fact]
    public async Task GetById_ForNonExistingId_Returns404NotFound()
    {
        // Arrange
        var id = Guid.NewGuid();
        _repositoryMock.Setup(repo => repo.GetByIdAsync(id))
            .ReturnsAsync((Domain.Entities.MediaLibrary)null);

        var client = _factory.CreateClient();

        // Act
        var result = await client.GetAsync($"api/media-library/{id}", TestContext.Current.CancellationToken);

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Create_ForValidCommand_Returns201Created()
    {
        // Arrange
        var id = Guid.NewGuid();
        _repositoryMock.Setup(repo => repo.CreateAsync(It.IsAny<Domain.Entities.MediaLibrary>()))
            .ReturnsAsync(id);
        var client = _factory.CreateClient();

        var command = new CreateMediaLibraryCommand { CategoryId = Guid.NewGuid(), StatusId = Guid.NewGuid(), Title = "Test Title", Details = "Test Details" };

        // Act
        var result = await client.PostAsJsonAsync("api/media-library", command, TestContext.Current.CancellationToken);

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.Created);
        result.Headers.Location!.ToString().Should().EndWith($"/api/media-library/{id}");
        _repositoryMock.Verify(repo => repo.CreateAsync(It.IsAny<Domain.Entities.MediaLibrary>()), Times.Once);
    }

    [Fact]
    public async Task Create_ForInvalidCommand_Returns400BadRequest()
    {
        // Arrange
        var client = _factory.CreateClient();

        var command = new CreateMediaLibraryCommand { Title = "" };

        // Act
        var result = await client.PostAsJsonAsync("api/media-library", command, TestContext.Current.CancellationToken);

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        _repositoryMock.Verify(repo => repo.CreateAsync(It.IsAny<Domain.Entities.MediaLibrary>()), Times.Never);
    }

    [Fact]
    public async Task Update_ForExistingId_Returns204NoContent()
    {
        // Arrange
        var id = Guid.NewGuid();
        _repositoryMock.Setup(repo => repo.GetByIdAsync(id))
            .ReturnsAsync(new Domain.Entities.MediaLibrary { MediaId = id });

        var client = _factory.CreateClient();

        var command = new UpdateMediaLibraryCommand { CategoryId = Guid.NewGuid(), StatusId = Guid.NewGuid(), Title = "Updated Title", Details = "Updated Details" };

        // Act
        var result = await client.PatchAsJsonAsync($"api/media-library/{id}", command, TestContext.Current.CancellationToken);

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.NoContent);
        _repositoryMock.Verify(repo => repo.SaveChanges(), Times.Once);
    }

    [Fact]
    public async Task Update_ForNonExistingId_Returns404NotFound()
    {
        // Arrange
        var id = Guid.NewGuid();
        _repositoryMock.Setup(repo => repo.GetByIdAsync(id))
            .ReturnsAsync((Domain.Entities.MediaLibrary)null);

        var client = _factory.CreateClient();

        var command = new UpdateMediaLibraryCommand { CategoryId = Guid.NewGuid(), StatusId = Guid.NewGuid(), Title = "Updated Title", Details = "Updated Details" };

        // Act
        var result = await client.PatchAsJsonAsync($"api/media-library/{id}", command, TestContext.Current.CancellationToken);

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Update_ForInvalidCommand_Returns400BadRequest()
    {
        // Arrange
        var id = Guid.NewGuid();
        var client = _factory.CreateClient();

        var command = new UpdateMediaLibraryCommand { Title = "" };

        // Act
        var result = await client.PatchAsJsonAsync($"api/media-library/{id}", command, TestContext.Current.CancellationToken);

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
            .ReturnsAsync(new Domain.Entities.MediaLibrary { MediaId = id, IsActive = true });

        var client = _factory.CreateClient();

        var command = new ToggleMediaLibraryCommand { IsActive = false };

        // Act
        var result = await client.PatchAsJsonAsync($"api/media-library/toggle/{id}", command, TestContext.Current.CancellationToken);

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
            .ReturnsAsync((Domain.Entities.MediaLibrary)null);

        var client = _factory.CreateClient();

        var command = new ToggleMediaLibraryCommand { IsActive = false };

        // Act
        var result = await client.PatchAsJsonAsync($"api/media-library/toggle/{id}", command, TestContext.Current.CancellationToken);

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Delete_ForExistingId_Returns204NoContent()
    {
        // Arrange
        var id = Guid.NewGuid();
        _repositoryMock.Setup(repo => repo.GetByIdAsync(id))
            .ReturnsAsync(new Domain.Entities.MediaLibrary { MediaId = id });

        var client = _factory.CreateClient();

        // Act
        var result = await client.DeleteAsync($"api/media-library/{id}", TestContext.Current.CancellationToken);

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.NoContent);
        _repositoryMock.Verify(repo => repo.DeleteAsync(It.Is<Domain.Entities.MediaLibrary>(data => data.MediaId == id)), Times.Once);
    }

    [Fact]
    public async Task Delete_ForNonExistingId_Returns404NotFound()
    {
        // Arrange
        var id = Guid.NewGuid();
        _repositoryMock.Setup(repo => repo.GetByIdAsync(id))
            .ReturnsAsync((Domain.Entities.MediaLibrary)null);

        var client = _factory.CreateClient();

        // Act
        var result = await client.DeleteAsync($"api/media-library/{id}", TestContext.Current.CancellationToken);

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.NotFound);
        _repositoryMock.Verify(repo => repo.DeleteAsync(It.IsAny<Domain.Entities.MediaLibrary>()), Times.Never);
    }
}
