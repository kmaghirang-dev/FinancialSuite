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
using FinancialSuite.Application.DropDownDetails.Commands.Create;
using FinancialSuite.Application.DropDownDetails.Commands.Toggle;
using FinancialSuite.Application.DropDownDetails.Commands.Update;
using FinancialSuite.Application.DropDownDetails.Dtos;
using FinancialSuite.Domain.Entities;
using FinancialSuite.Domain.Repositories;
using Xunit;

namespace FinancialSuite.API.Tests.Controllers;

[TestSubject(typeof(DropDownDetailsController))]
public class DropDownDetailsControllerTest : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly Mock<IDropDownCategoryRepository> _categoryRepositoryMock = new();
    private readonly Mock<IDropDownDetailsRepository> _detailsRepositoryMock = new();

    public DropDownDetailsControllerTest(WebApplicationFactory<Program> factory)
    {
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                services.Replace(ServiceDescriptor.Singleton<IPolicyEvaluator, FakePolicyEvaluator>());
                services.Replace(ServiceDescriptor.Scoped(typeof(IDropDownCategoryRepository), _ => _categoryRepositoryMock.Object));
                services.Replace(ServiceDescriptor.Scoped(typeof(IDropDownDetailsRepository), _ => _detailsRepositoryMock.Object));
            });
        });
    }

    private (Guid categoryId, DropDownDetail detail) SetupCategoryWithDetail()
    {
        var categoryId = Guid.NewGuid();
        var detail = new DropDownDetail
        {
            DropDownDetailId = Guid.NewGuid(),
            DropDownCategoryId = categoryId,
            Code = "CODE",
            DisplayValue = "Display Value",
            IsActive = true
        };
        _categoryRepositoryMock.Setup(repo => repo.GetByIdAsync(categoryId))
            .ReturnsAsync(new DropDownCategory { DropDownCategoryId = categoryId, DropDownDetails = [detail] });
        return (categoryId, detail);
    }

    private Guid SetupMissingCategory()
    {
        var categoryId = Guid.NewGuid();
        _categoryRepositoryMock.Setup(repo => repo.GetByIdAsync(categoryId))
            .ReturnsAsync((DropDownCategory)null);
        return categoryId;
    }

    [Fact]
    public async Task Create_ForValidCommand_Returns201Created()
    {
        // Arrange
        var (categoryId, _) = SetupCategoryWithDetail();
        var client = _factory.CreateClient();

        var command = new CreateDropDownDetailsCommand { Code = "NEW", DisplayValue = "New Value", CreatedBy = "Creator" };

        // Act
        var result = await client.PostAsJsonAsync($"api/dropdown/category/{categoryId}/details", command, TestContext.Current.CancellationToken);

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.Created);
        _detailsRepositoryMock.Verify(repo => repo.CreateAsync(
            It.Is<DropDownDetail>(detail => detail.DropDownCategoryId == categoryId && detail.Code == "NEW")), Times.Once);
    }

    [Fact]
    public async Task Create_ForInvalidCommand_Returns400BadRequest()
    {
        // Arrange
        var (categoryId, _) = SetupCategoryWithDetail();
        var client = _factory.CreateClient();

        var command = new CreateDropDownDetailsCommand { Code = "", DisplayValue = "", CreatedBy = "" };

        // Act
        var result = await client.PostAsJsonAsync($"api/dropdown/category/{categoryId}/details", command, TestContext.Current.CancellationToken);

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        _detailsRepositoryMock.Verify(repo => repo.CreateAsync(It.IsAny<DropDownDetail>()), Times.Never);
    }

    [Fact]
    public async Task Create_ForNonExistingCategory_Returns404NotFound()
    {
        // Arrange
        var categoryId = SetupMissingCategory();
        var client = _factory.CreateClient();

        var command = new CreateDropDownDetailsCommand { Code = "NEW", DisplayValue = "New Value", CreatedBy = "Creator" };

        // Act
        var result = await client.PostAsJsonAsync($"api/dropdown/category/{categoryId}/details", command, TestContext.Current.CancellationToken);

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetAllByCategoryId_ForExistingCategory_Returns200Ok()
    {
        // Arrange
        var (categoryId, detail) = SetupCategoryWithDetail();
        var client = _factory.CreateClient();

        // Act
        var result = await client.GetAsync($"api/dropdown/category/{categoryId}/details", TestContext.Current.CancellationToken);
        var returned = await result.Content.ReadFromJsonAsync<List<GetAllDropDownDetailsDto>>(TestContext.Current.CancellationToken);

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.OK);
        returned.Should().ContainSingle()
            .Which.DropDownDetailId.Should().Be(detail.DropDownDetailId);
    }

    [Fact]
    public async Task GetAllByCategoryId_ForNonExistingCategory_Returns404NotFound()
    {
        // Arrange
        var categoryId = SetupMissingCategory();
        var client = _factory.CreateClient();

        // Act
        var result = await client.GetAsync($"api/dropdown/category/{categoryId}/details", TestContext.Current.CancellationToken);

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetAllPaginated_ForValidRequest_Returns200Ok()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var client = _factory.CreateClient();

        // Act
        var result = await client.GetAsync($"api/dropdown/category/{categoryId}/details/paginated?pageNumber=1&pageSize=10", TestContext.Current.CancellationToken);

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetAllPaginated_ForInvalidRequest_Returns400BadRequest()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var client = _factory.CreateClient();

        // Act
        var result = await client.GetAsync($"api/dropdown/category/{categoryId}/details/paginated", TestContext.Current.CancellationToken);

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetById_ForExistingDetail_Returns200Ok()
    {
        // Arrange
        var (categoryId, detail) = SetupCategoryWithDetail();
        var client = _factory.CreateClient();

        // Act
        var result = await client.GetAsync($"api/dropdown/category/{categoryId}/details/{detail.DropDownDetailId}", TestContext.Current.CancellationToken);
        var returned = await result.Content.ReadFromJsonAsync<GetByIdDropDownDetailsDto>(TestContext.Current.CancellationToken);

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.OK);
        returned.Should().NotBeNull();
        returned!.DropDownDetailId.Should().Be(detail.DropDownDetailId);
        returned.Code.Should().Be(detail.Code);
    }

    [Fact]
    public async Task GetById_ForNonExistingDetail_Returns404NotFound()
    {
        // Arrange
        var (categoryId, _) = SetupCategoryWithDetail();
        var client = _factory.CreateClient();

        // Act
        var result = await client.GetAsync($"api/dropdown/category/{categoryId}/details/{Guid.NewGuid()}", TestContext.Current.CancellationToken);

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetById_ForNonExistingCategory_Returns404NotFound()
    {
        // Arrange
        var categoryId = SetupMissingCategory();
        var client = _factory.CreateClient();

        // Act
        var result = await client.GetAsync($"api/dropdown/category/{categoryId}/details/{Guid.NewGuid()}", TestContext.Current.CancellationToken);

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Delete_ForExistingCategory_Returns204NoContent()
    {
        // Arrange
        var (categoryId, detail) = SetupCategoryWithDetail();
        var client = _factory.CreateClient();

        // Act
        var result = await client.DeleteAsync($"api/dropdown/category/{categoryId}/details", TestContext.Current.CancellationToken);

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.NoContent);
        _detailsRepositoryMock.Verify(repo => repo.DeleteAsync(
            It.Is<IEnumerable<DropDownDetail>>(details => details != null)), Times.Once);
    }

    [Fact]
    public async Task Delete_ForNonExistingCategory_Returns404NotFound()
    {
        // Arrange
        var categoryId = SetupMissingCategory();
        var client = _factory.CreateClient();

        // Act
        var result = await client.DeleteAsync($"api/dropdown/category/{categoryId}/details", TestContext.Current.CancellationToken);

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DeleteById_ForExistingDetail_Returns204NoContent()
    {
        // Arrange
        var (categoryId, detail) = SetupCategoryWithDetail();
        var client = _factory.CreateClient();

        // Act
        var result = await client.DeleteAsync($"api/dropdown/category/{categoryId}/details/{detail.DropDownDetailId}", TestContext.Current.CancellationToken);

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.NoContent);
        _detailsRepositoryMock.Verify(repo => repo.DeleteAsync(It.IsAny<IEnumerable<DropDownDetail>>()), Times.Once);
    }

    [Fact]
    public async Task DeleteById_ForNonExistingDetail_Returns404NotFound()
    {
        // Arrange
        var (categoryId, _) = SetupCategoryWithDetail();
        var client = _factory.CreateClient();

        // Act
        var result = await client.DeleteAsync($"api/dropdown/category/{categoryId}/details/{Guid.NewGuid()}", TestContext.Current.CancellationToken);

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.NotFound);
        _detailsRepositoryMock.Verify(repo => repo.DeleteAsync(It.IsAny<IEnumerable<DropDownDetail>>()), Times.Never);
    }

    [Fact]
    public async Task Update_ForExistingDetail_Returns204NoContent()
    {
        // Arrange
        var (categoryId, detail) = SetupCategoryWithDetail();
        var client = _factory.CreateClient();

        var command = new UpdateDropDownDetailsCommand { Code = "NEW", DisplayValue = "New Value", UpdatedBy = "Updater" };

        // Act
        var result = await client.PatchAsJsonAsync($"api/dropdown/category/{categoryId}/details/{detail.DropDownDetailId}", command, TestContext.Current.CancellationToken);

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.NoContent);
        detail.Code.Should().Be("NEW");
        detail.DisplayValue.Should().Be("New Value");
        detail.UpdatedBy.Should().Be("Updater");
        _detailsRepositoryMock.Verify(repo => repo.SaveChanges(), Times.Once);
    }

    [Fact]
    public async Task Update_ForInvalidCommand_Returns400BadRequest()
    {
        // Arrange
        var (categoryId, detail) = SetupCategoryWithDetail();
        var client = _factory.CreateClient();

        var command = new UpdateDropDownDetailsCommand();

        // Act
        var result = await client.PatchAsJsonAsync($"api/dropdown/category/{categoryId}/details/{detail.DropDownDetailId}", command, TestContext.Current.CancellationToken);

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        _detailsRepositoryMock.Verify(repo => repo.SaveChanges(), Times.Never);
    }

    [Fact]
    public async Task Update_ForNonExistingDetail_Returns404NotFound()
    {
        // Arrange
        var (categoryId, _) = SetupCategoryWithDetail();
        var client = _factory.CreateClient();

        var command = new UpdateDropDownDetailsCommand { Code = "NEW", DisplayValue = "New Value", UpdatedBy = "Updater" };

        // Act
        var result = await client.PatchAsJsonAsync($"api/dropdown/category/{categoryId}/details/{Guid.NewGuid()}", command, TestContext.Current.CancellationToken);

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Toggle_ForExistingDetail_Returns204NoContent()
    {
        // Arrange
        var (categoryId, detail) = SetupCategoryWithDetail();
        var client = _factory.CreateClient();

        var command = new ToggleDropDownDetailsCommand { UpdatedBy = "Updater", IsActive = false };

        // Act
        var result = await client.PatchAsJsonAsync($"api/dropdown/category/{categoryId}/details/toggle/{detail.DropDownDetailId}", command, TestContext.Current.CancellationToken);

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.NoContent);
        detail.IsActive.Should().BeFalse();
        _detailsRepositoryMock.Verify(repo => repo.SaveChanges(), Times.Once);
    }

    [Fact]
    public async Task Toggle_ForNonExistingCategory_Returns404NotFound()
    {
        // Arrange
        var categoryId = SetupMissingCategory();
        var client = _factory.CreateClient();

        var command = new ToggleDropDownDetailsCommand { UpdatedBy = "Updater", IsActive = false };

        // Act
        var result = await client.PatchAsJsonAsync($"api/dropdown/category/{categoryId}/details/toggle/{Guid.NewGuid()}", command, TestContext.Current.CancellationToken);

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
