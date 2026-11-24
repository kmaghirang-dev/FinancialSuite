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
using FinancialSuite.Application.DropDownDetails.Commands.DeleteById;
using FinancialSuite.Domain.Entities;
using FinancialSuite.Domain.Exceptions;
using FinancialSuite.Domain.Repositories;

namespace FinancialSuite.Application.Tests.DropDownDetails.Commands.DeleteById;

[TestSubject(typeof(DeleteByIdDropDownDetailsCommandHandler))]
public class DeleteByIdDropDownDetailsCommandHandlerTest
{
    private readonly Mock<IDropDownCategoryRepository> _categoryRepositoryMock;
    private readonly Mock<IDropDownDetailsRepository> _detailsRepositoryMock;
    private readonly DeleteByIdDropDownDetailsCommandHandler _handler;

    public DeleteByIdDropDownDetailsCommandHandlerTest()
    {
        var loggerMock = new Mock<ILogger<DeleteByIdDropDownDetailsCommandHandler>>();
        var mapperMock = new Mock<IMapper>();
        _categoryRepositoryMock = new Mock<IDropDownCategoryRepository>();
        _detailsRepositoryMock = new Mock<IDropDownDetailsRepository>();
        _handler = new DeleteByIdDropDownDetailsCommandHandler(
            loggerMock.Object,
            mapperMock.Object,
            _categoryRepositoryMock.Object,
            _detailsRepositoryMock.Object);
    }

    [Fact]
    public async Task Handle_ForExistingDetail_DeletesOnlyThatDetail()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var detailId = Guid.NewGuid();
        var detail = new DropDownDetail { DropDownDetailId = detailId };
        var otherDetail = new DropDownDetail { DropDownDetailId = Guid.NewGuid() };
        var category = new Domain.Entities.DropDownCategory
        {
            DropDownCategoryId = categoryId,
            DropDownDetails = [detail, otherDetail]
        };

        _categoryRepositoryMock.Setup(repo => repo.GetByIdAsync(categoryId))
            .ReturnsAsync(category);

        // Act
        await _handler.Handle(new DeleteByIdDropDownDetailsCommand(categoryId, detailId), CancellationToken.None);

        // Assert
        _detailsRepositoryMock.Verify(repo => repo.DeleteAsync(
            It.Is<IEnumerable<DropDownDetail>>(entities => entities.Count() == 1 && entities.Single() == detail)), Times.Once);
    }

    [Fact]
    public async Task Handle_WithNonExistingDropDownCategory_ShouldThrowNotFoundException()
    {
        // Arrange
        var categoryId = Guid.NewGuid();

        _categoryRepositoryMock.Setup(repo => repo.GetByIdAsync(categoryId))
            .ReturnsAsync((Domain.Entities.DropDownCategory)null);

        // Act
        Func<Task> act = () => _handler.Handle(new DeleteByIdDropDownDetailsCommand(categoryId, Guid.NewGuid()), CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage($"The DropDownCategory with ID {categoryId} was not found.");
        _detailsRepositoryMock.Verify(repo => repo.DeleteAsync(It.IsAny<IEnumerable<DropDownDetail>>()), Times.Never);
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

        _categoryRepositoryMock.Setup(repo => repo.GetByIdAsync(categoryId))
            .ReturnsAsync(category);

        // Act
        Func<Task> act = () => _handler.Handle(new DeleteByIdDropDownDetailsCommand(categoryId, detailId), CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage($"The DropDownDetails with ID {detailId} was not found.");
        _detailsRepositoryMock.Verify(repo => repo.DeleteAsync(It.IsAny<IEnumerable<DropDownDetail>>()), Times.Never);
    }
}
