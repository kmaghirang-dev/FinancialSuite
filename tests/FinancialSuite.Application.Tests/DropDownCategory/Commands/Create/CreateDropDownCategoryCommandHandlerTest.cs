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
using FinancialSuite.Domain.Repositories;

namespace FinancialSuite.Application.Tests.DropDownCategory.Commands.Create;

[TestSubject(typeof(CreateDropDownCategoryCommandHandler))]
public class CreateDropDownCategoryCommandHandlerTest
{
    private readonly Mock<IMapper> _mapperMock;
    private readonly Mock<IDropDownCategoryRepository> _repositoryMock;
    private readonly CreateDropDownCategoryCommandHandler _handler;

    public CreateDropDownCategoryCommandHandlerTest()
    {
        var loggerMock = new Mock<ILogger<CreateDropDownCategoryCommandHandler>>();
        _mapperMock = new Mock<IMapper>();
        _repositoryMock = new Mock<IDropDownCategoryRepository>();
        _handler = new CreateDropDownCategoryCommandHandler(
            loggerMock.Object,
            _mapperMock.Object,
            _repositoryMock.Object);
    }

    [Fact]
    public async Task Handle_ForValidCommand_ReturnsDropDownCategoryId()
    {
        // Arrange
        var command = new CreateDropDownCategoryCommand
        {
            Value = "Test Value",
            CreatedBy = "Creator"
        };
        var data = new Domain.Entities.DropDownCategory();

        _mapperMock.Setup(mapper => mapper.Map<Domain.Entities.DropDownCategory>(command))
            .Returns(data);

        var expectedId = Guid.NewGuid();
        _repositoryMock.Setup(repo => repo.CreateAsync(data))
            .ReturnsAsync(expectedId);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().Be(expectedId);
        _repositoryMock.Verify(repo => repo.CreateAsync(data), Times.Once);
    }
}
