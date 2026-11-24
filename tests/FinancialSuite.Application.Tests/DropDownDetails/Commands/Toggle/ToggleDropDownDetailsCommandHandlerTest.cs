using System;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using FluentAssertions;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using FinancialSuite.Application.DropDownDetails.Commands.DeleteById;
using FinancialSuite.Application.DropDownDetails.Commands.Toggle;
using FinancialSuite.Domain.Entities;
using FinancialSuite.Domain.Exceptions;
using FinancialSuite.Domain.Repositories;

namespace FinancialSuite.Application.Tests.DropDownDetails.Commands.Toggle;

[TestSubject(typeof(ToggleDropDownDetailsCommandHandler))]
public class ToggleDropDownDetailsCommandHandlerTest
{
    private readonly Mock<IMapper> _mapperMock;
    private readonly Mock<IDropDownCategoryRepository> _categoryRepositoryMock;
    private readonly Mock<IDropDownDetailsRepository> _detailsRepositoryMock;
    private readonly ToggleDropDownDetailsCommandHandler _handler;

    public ToggleDropDownDetailsCommandHandlerTest()
    {
        // The handler's logger is typed with DeleteByIdDropDownDetailsCommandHandler (see CODING_GUIDELINES.md §13.1).
        var loggerMock = new Mock<ILogger<DeleteByIdDropDownDetailsCommandHandler>>();
        _mapperMock = new Mock<IMapper>();
        _categoryRepositoryMock = new Mock<IDropDownCategoryRepository>();
        _detailsRepositoryMock = new Mock<IDropDownDetailsRepository>();
        _handler = new ToggleDropDownDetailsCommandHandler(
            loggerMock.Object,
            _mapperMock.Object,
            _categoryRepositoryMock.Object,
            _detailsRepositoryMock.Object);
    }

    [Fact]
    public async Task Handle_ForExistingDetail_TogglesDetail()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var detailId = Guid.NewGuid();
        var detail = new DropDownDetail { DropDownDetailId = detailId, IsActive = true };
        var category = new Domain.Entities.DropDownCategory
        {
            DropDownCategoryId = categoryId,
            DropDownDetails = [detail]
        };
        var command = new ToggleDropDownDetailsCommand
        {
            DropDownCategoryId = categoryId,
            DropDownDetailId = detailId,
            UpdatedBy = "Updater",
            IsActive = false
        };

        _categoryRepositoryMock.Setup(repo => repo.GetByIdAsync(categoryId))
            .ReturnsAsync(category);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _mapperMock.Verify(mapper => mapper.Map(command, detail), Times.Once);
        _detailsRepositoryMock.Verify(repo => repo.SaveChanges(), Times.Once);
    }

    [Fact]
    public async Task Handle_WithNonExistingDropDownCategory_ShouldThrowNotFoundException()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var command = new ToggleDropDownDetailsCommand
        {
            DropDownCategoryId = categoryId,
            DropDownDetailId = Guid.NewGuid()
        };

        _categoryRepositoryMock.Setup(repo => repo.GetByIdAsync(categoryId))
            .ReturnsAsync((Domain.Entities.DropDownCategory)null);

        // Act
        Func<Task> act = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage($"The DropDownCategory with ID {categoryId} was not found.");
        _detailsRepositoryMock.Verify(repo => repo.SaveChanges(), Times.Never);
    }
}
