using System;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using FluentAssertions;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using FinancialSuite.Application.DropDownCategory.Commands.Toggle;
using FinancialSuite.Domain.Exceptions;
using FinancialSuite.Domain.Repositories;

namespace FinancialSuite.Application.Tests.DropDownCategory.Commands.Toggle;

[TestSubject(typeof(ToggleDropDownCategoryCommandHandler))]
public class ToggleDropDownCategoryCommandHandlerTest
{
    private readonly Mock<IMapper> _mapperMock;
    private readonly Mock<IDropDownCategoryRepository> _repositoryMock;
    private readonly ToggleDropDownCategoryCommandHandler _handler;

    public ToggleDropDownCategoryCommandHandlerTest()
    {
        var loggerMock = new Mock<ILogger<ToggleDropDownCategoryCommandHandler>>();
        _mapperMock = new Mock<IMapper>();
        _repositoryMock = new Mock<IDropDownCategoryRepository>();
        _handler = new ToggleDropDownCategoryCommandHandler(
            loggerMock.Object,
            _mapperMock.Object,
            _repositoryMock.Object);
    }

    [Fact]
    public async Task Handle_ForValidCommand_TogglesDropDownCategory()
    {
        // Arrange
        var id = Guid.NewGuid();
        var command = new ToggleDropDownCategoryCommand
        {
            DropDownCategoryId = id,
            UpdatedBy = "Updater",
            IsActive = false
        };
        var data = new Domain.Entities.DropDownCategory { DropDownCategoryId = id, IsActive = true };

        _repositoryMock.Setup(repo => repo.GetByIdAsync(id))
            .ReturnsAsync(data);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _mapperMock.Verify(mapper => mapper.Map(command, data), Times.Once);
        _repositoryMock.Verify(repo => repo.SaveChanges(), Times.Once);
    }

    [Fact]
    public async Task Handle_WithNonExistingDropDownCategory_ShouldThrowNotFoundException()
    {
        // Arrange
        var id = Guid.NewGuid();
        var command = new ToggleDropDownCategoryCommand { DropDownCategoryId = id };

        _repositoryMock.Setup(repo => repo.GetByIdAsync(id))
            .ReturnsAsync((Domain.Entities.DropDownCategory)null);

        // Act
        Func<Task> act = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage($"The DropDownCategory with ID {id} was not found.");
        _repositoryMock.Verify(repo => repo.SaveChanges(), Times.Never);
    }
}
