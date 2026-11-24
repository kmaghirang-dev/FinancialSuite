using System;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using FluentAssertions;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using FinancialSuite.Application.DropDownDetails.Commands.Update;
using FinancialSuite.Domain.Entities;
using FinancialSuite.Domain.Exceptions;
using FinancialSuite.Domain.Repositories;

namespace FinancialSuite.Application.Tests.DropDownDetails.Commands.Update;

[TestSubject(typeof(UpdateDropDownDetailsCommandHandler))]
public class UpdateDropDownDetailsCommandHandlerTest
{
    private readonly Mock<IMapper> _mapperMock;
    private readonly Mock<IDropDownCategoryRepository> _categoryRepositoryMock;
    private readonly Mock<IDropDownDetailsRepository> _detailsRepositoryMock;
    private readonly UpdateDropDownDetailsCommandHandler _handler;

    public UpdateDropDownDetailsCommandHandlerTest()
    {
        var loggerMock = new Mock<ILogger<UpdateDropDownDetailsCommandHandler>>();
        _mapperMock = new Mock<IMapper>();
        _categoryRepositoryMock = new Mock<IDropDownCategoryRepository>();
        _detailsRepositoryMock = new Mock<IDropDownDetailsRepository>();
        _handler = new UpdateDropDownDetailsCommandHandler(
            loggerMock.Object,
            _mapperMock.Object,
            _categoryRepositoryMock.Object,
            _detailsRepositoryMock.Object);
    }

    [Fact]
    public async Task Handle_ForExistingDetail_UpdatesDetail()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var detailId = Guid.NewGuid();
        var detail = new DropDownDetail { DropDownDetailId = detailId };
        var category = new Domain.Entities.DropDownCategory
        {
            DropDownCategoryId = categoryId,
            DropDownDetails = [detail]
        };
        var command = new UpdateDropDownDetailsCommand
        {
            DropDownCategoryId = categoryId,
            DropDownDetailId = detailId,
            Code = "NEW",
            DisplayValue = "New Value",
            UpdatedBy = "Updater"
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
        var command = new UpdateDropDownDetailsCommand
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

    [Fact]
    public async Task Handle_WithNonExistingDetail_ShouldThrowNotFoundException()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var detailId = Guid.NewGuid();
        var category = new Domain.Entities.DropDownCategory
        {
            DropDownCategoryId = categoryId,
            DropDownDetails = [new DropDownDetail { DropDownDetailId = Guid.NewGuid() }]
        };
        var command = new UpdateDropDownDetailsCommand
        {
            DropDownCategoryId = categoryId,
            DropDownDetailId = detailId
        };

        _categoryRepositoryMock.Setup(repo => repo.GetByIdAsync(categoryId))
            .ReturnsAsync(category);

        // Act
        Func<Task> act = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage($"The DropDownDetails with ID {detailId} was not found.");
        _detailsRepositoryMock.Verify(repo => repo.SaveChanges(), Times.Never);
    }
}
