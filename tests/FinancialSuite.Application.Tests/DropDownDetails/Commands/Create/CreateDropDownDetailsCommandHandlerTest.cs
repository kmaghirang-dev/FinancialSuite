using System;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using FluentAssertions;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using FinancialSuite.Application.DropDownDetails.Commands.Create;
using FinancialSuite.Domain.Entities;
using FinancialSuite.Domain.Exceptions;
using FinancialSuite.Domain.Repositories;

namespace FinancialSuite.Application.Tests.DropDownDetails.Commands.Create;

[TestSubject(typeof(CreateDropDownDetailsCommandHandler))]
public class CreateDropDownDetailsCommandHandlerTest
{
    private readonly Mock<IMapper> _mapperMock;
    private readonly Mock<IDropDownCategoryRepository> _categoryRepositoryMock;
    private readonly Mock<IDropDownDetailsRepository> _detailsRepositoryMock;
    private readonly CreateDropDownDetailsCommandHandler _handler;

    public CreateDropDownDetailsCommandHandlerTest()
    {
        var loggerMock = new Mock<ILogger<CreateDropDownDetailsCommandHandler>>();
        _mapperMock = new Mock<IMapper>();
        _categoryRepositoryMock = new Mock<IDropDownCategoryRepository>();
        _detailsRepositoryMock = new Mock<IDropDownDetailsRepository>();
        _handler = new CreateDropDownDetailsCommandHandler(
            loggerMock.Object,
            _mapperMock.Object,
            _categoryRepositoryMock.Object,
            _detailsRepositoryMock.Object);
    }

    [Fact]
    public async Task Handle_ForExistingDropDownCategory_CreatesDropDownDetail()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var command = new CreateDropDownDetailsCommand
        {
            DropDownCategoryId = categoryId,
            Code = "CODE",
            DisplayValue = "Display Value",
            CreatedBy = "Creator"
        };
        var detail = new DropDownDetail { DropDownCategoryId = categoryId };

        _categoryRepositoryMock.Setup(repo => repo.GetByIdAsync(categoryId))
            .ReturnsAsync(new Domain.Entities.DropDownCategory { DropDownCategoryId = categoryId });
        _mapperMock.Setup(mapper => mapper.Map<DropDownDetail>(command))
            .Returns(detail);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _detailsRepositoryMock.Verify(repo => repo.CreateAsync(detail), Times.Once);
    }

    [Fact]
    public async Task Handle_WithNonExistingDropDownCategory_ShouldThrowNotFoundException()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var command = new CreateDropDownDetailsCommand { DropDownCategoryId = categoryId };

        _categoryRepositoryMock.Setup(repo => repo.GetByIdAsync(categoryId))
            .ReturnsAsync((Domain.Entities.DropDownCategory)null);

        // Act
        Func<Task> act = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
        _detailsRepositoryMock.Verify(repo => repo.CreateAsync(It.IsAny<DropDownDetail>()), Times.Never);
    }
}
