using System;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using FluentAssertions;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using FinancialSuite.Application.DropDownCategory.Commands.Create;
using FinancialSuite.Application.DropDownCategory.Commands.Update;
using FinancialSuite.Domain.Exceptions;
using FinancialSuite.Domain.Repositories;

namespace FinancialSuite.Application.Tests.DropDownCategory.Commands.Update;

[TestSubject(typeof(UpdateDropDownCategoryCommandHandler))]
public class UpdateDropDownCategoryCommandHandlerTest
{
    private readonly Mock<IMapper> _mapperMock;
    private readonly Mock<IDropDownCategoryRepository> _repositoryMock;
    private readonly UpdateDropDownCategoryCommandHandler _handler;

    public UpdateDropDownCategoryCommandHandlerTest()
    {
        // The handler's logger is typed with CreateDropDownCategoryCommandHandler (see CODING_GUIDELINES.md §13.1).
        var loggerMock = new Mock<ILogger<CreateDropDownCategoryCommandHandler>>();
        _mapperMock = new Mock<IMapper>();
        _repositoryMock = new Mock<IDropDownCategoryRepository>();
        _handler = new UpdateDropDownCategoryCommandHandler(
            loggerMock.Object,
            _mapperMock.Object,
            _repositoryMock.Object);
    }

    [Fact]
    public async Task Handle_ForValidCommand_UpdatesDropDownCategory()
    {
        // Arrange
        var id = Guid.NewGuid();
        var command = new UpdateDropDownCategoryCommand
        {
            DropDownCategoryId = id,
            Value = "Updated Value",
            UpdatedBy = "Updater"
        };
        var data = new Domain.Entities.DropDownCategory { DropDownCategoryId = id };

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
        var command = new UpdateDropDownCategoryCommand { DropDownCategoryId = id };

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
