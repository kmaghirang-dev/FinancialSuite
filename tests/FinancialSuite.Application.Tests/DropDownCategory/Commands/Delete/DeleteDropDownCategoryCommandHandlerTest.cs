using System;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using FluentAssertions;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using FinancialSuite.Application.DropDownCategory.Commands.Delete;
using FinancialSuite.Domain.Exceptions;
using FinancialSuite.Domain.Repositories;

namespace FinancialSuite.Application.Tests.DropDownCategory.Commands.Delete;

[TestSubject(typeof(DeleteDropDownCategoryCommandHandler))]
public class DeleteDropDownCategoryCommandHandlerTest
{
    private readonly Mock<IDropDownCategoryRepository> _repositoryMock;
    private readonly DeleteDropDownCategoryCommandHandler _handler;

    public DeleteDropDownCategoryCommandHandlerTest()
    {
        var loggerMock = new Mock<ILogger<DeleteDropDownCategoryCommandHandler>>();
        var mapperMock = new Mock<IMapper>();
        _repositoryMock = new Mock<IDropDownCategoryRepository>();
        _handler = new DeleteDropDownCategoryCommandHandler(
            loggerMock.Object,
            mapperMock.Object,
            _repositoryMock.Object);
    }

    [Fact]
    public async Task Handle_ForExistingDropDownCategory_DeletesDropDownCategory()
    {
        // Arrange
        var id = Guid.NewGuid();
        var data = new Domain.Entities.DropDownCategory { DropDownCategoryId = id };

        _repositoryMock.Setup(repo => repo.GetByIdAsync(id))
            .ReturnsAsync(data);

        // Act
        await _handler.Handle(new DeleteDropDownCategoryCommand(id), CancellationToken.None);

        // Assert
        _repositoryMock.Verify(repo => repo.DeleteAsync(data), Times.Once);
    }

    [Fact]
    public async Task Handle_WithNonExistingDropDownCategory_ShouldThrowNotFoundException()
    {
        // Arrange
        var id = Guid.NewGuid();

        _repositoryMock.Setup(repo => repo.GetByIdAsync(id))
            .ReturnsAsync((Domain.Entities.DropDownCategory)null);

        // Act
        Func<Task> act = () => _handler.Handle(new DeleteDropDownCategoryCommand(id), CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage($"The DropDownCategory with ID {id} was not found.");
        _repositoryMock.Verify(repo => repo.DeleteAsync(It.IsAny<Domain.Entities.DropDownCategory>()), Times.Never);
    }
}
