using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using FluentAssertions;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using FinancialSuite.Application.DropDownDetails.Commands.DeleteAllByCategory;
using FinancialSuite.Domain.Entities;
using FinancialSuite.Domain.Exceptions;
using FinancialSuite.Domain.Repositories;

namespace FinancialSuite.Application.Tests.DropDownDetails.Commands.DeleteAllByCategory;

[TestSubject(typeof(DeleteAllDropDownDetailsByCategoryCommandHandler))]
public class DeleteAllDropDownDetailsByCategoryCommandHandlerTest
{
    private readonly Mock<IDropDownCategoryRepository> _categoryRepositoryMock;
    private readonly Mock<IDropDownDetailsRepository> _detailsRepositoryMock;
    private readonly DeleteAllDropDownDetailsByCategoryCommandHandler _handler;

    public DeleteAllDropDownDetailsByCategoryCommandHandlerTest()
    {
        var loggerMock = new Mock<ILogger<DeleteAllDropDownDetailsByCategoryCommandHandler>>();
        var mapperMock = new Mock<IMapper>();
        _categoryRepositoryMock = new Mock<IDropDownCategoryRepository>();
        _detailsRepositoryMock = new Mock<IDropDownDetailsRepository>();
        _handler = new DeleteAllDropDownDetailsByCategoryCommandHandler(
            loggerMock.Object,
            mapperMock.Object,
            _categoryRepositoryMock.Object,
            _detailsRepositoryMock.Object);
    }

    [Fact]
    public async Task Handle_ForExistingDropDownCategory_DeletesAllDetails()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var details = new List<DropDownDetail> { new DropDownDetail(), new DropDownDetail() };
        var category = new Domain.Entities.DropDownCategory
        {
            DropDownCategoryId = categoryId,
            DropDownDetails = details
        };

        _categoryRepositoryMock.Setup(repo => repo.GetByIdAsync(categoryId))
            .ReturnsAsync(category);

        // Act
        await _handler.Handle(new DeleteAllDropDownDetailsByCategoryCommand(categoryId), CancellationToken.None);

        // Assert
        _detailsRepositoryMock.Verify(repo => repo.DeleteAsync(details), Times.Once);
    }

    [Fact]
    public async Task Handle_ForDropDownCategoryWithoutDetails_DeletesEmptyList()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var category = new Domain.Entities.DropDownCategory
        {
            DropDownCategoryId = categoryId,
            DropDownDetails = null
        };

        _categoryRepositoryMock.Setup(repo => repo.GetByIdAsync(categoryId))
            .ReturnsAsync(category);

        // Act
        await _handler.Handle(new DeleteAllDropDownDetailsByCategoryCommand(categoryId), CancellationToken.None);

        // Assert
        _detailsRepositoryMock.Verify(repo => repo.DeleteAsync(
            It.Is<IEnumerable<DropDownDetail>>(entities => !entities.Any())), Times.Once);
    }

    [Fact]
    public async Task Handle_WithNonExistingDropDownCategory_ShouldThrowNotFoundException()
    {
        // Arrange
        var categoryId = Guid.NewGuid();

        _categoryRepositoryMock.Setup(repo => repo.GetByIdAsync(categoryId))
            .ReturnsAsync((Domain.Entities.DropDownCategory)null);

        // Act
        Func<Task> act = () => _handler.Handle(new DeleteAllDropDownDetailsByCategoryCommand(categoryId), CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage($"The DropDownCategory with ID {categoryId} was not found.");
        _detailsRepositoryMock.Verify(repo => repo.DeleteAsync(It.IsAny<IEnumerable<DropDownDetail>>()), Times.Never);
    }
}
