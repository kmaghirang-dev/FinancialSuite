using System;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using FluentAssertions;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using FinancialSuite.Application.DropDownDetails.Dtos;
using FinancialSuite.Application.DropDownDetails.Queries.GetById;
using FinancialSuite.Domain.Entities;
using FinancialSuite.Domain.Exceptions;
using FinancialSuite.Domain.Repositories;

namespace FinancialSuite.Application.Tests.DropDownDetails.Queries.GetById;

[TestSubject(typeof(GetByIdDropDownDetailsQueryHandler))]
public class GetByIdDropDownDetailsQueryHandlerTest
{
    private readonly Mock<IMapper> _mapperMock;
    private readonly Mock<IDropDownCategoryRepository> _repositoryMock;
    private readonly GetByIdDropDownDetailsQueryHandler _handler;

    public GetByIdDropDownDetailsQueryHandlerTest()
    {
        var loggerMock = new Mock<ILogger<GetByIdDropDownDetailsQueryHandler>>();
        _mapperMock = new Mock<IMapper>();
        _repositoryMock = new Mock<IDropDownCategoryRepository>();
        _handler = new GetByIdDropDownDetailsQueryHandler(
            loggerMock.Object,
            _mapperMock.Object,
            _repositoryMock.Object);
    }

    [Fact]
    public async Task Handle_ForExistingDetail_ReturnsMappedDto()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var detailId = Guid.NewGuid();
        var detail = new DropDownDetail { DropDownDetailId = detailId };
        var category = new Domain.Entities.DropDownCategory
        {
            DropDownCategoryId = categoryId,
            DropDownDetails = [new DropDownDetail { DropDownDetailId = Guid.NewGuid() }, detail]
        };
        var dto = new GetByIdDropDownDetailsDto { DropDownDetailId = detailId };

        _repositoryMock.Setup(repo => repo.GetByIdAsync(categoryId))
            .ReturnsAsync(category);
        _mapperMock.Setup(mapper => mapper.Map<GetByIdDropDownDetailsDto>(detail))
            .Returns(dto);

        // Act
        var result = await _handler.Handle(new GetByIdDropDownDetailsQuery(categoryId, detailId), CancellationToken.None);

        // Assert
        result.Should().BeSameAs(dto);
    }

    [Fact]
    public async Task Handle_WithNonExistingDropDownCategory_ShouldThrowNotFoundException()
    {
        // Arrange
        var categoryId = Guid.NewGuid();

        _repositoryMock.Setup(repo => repo.GetByIdAsync(categoryId))
            .ReturnsAsync((Domain.Entities.DropDownCategory)null);

        // Act
        Func<Task> act = () => _handler.Handle(new GetByIdDropDownDetailsQuery(categoryId, Guid.NewGuid()), CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage($"The DropDownCategory with ID {categoryId} was not found.");
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

        _repositoryMock.Setup(repo => repo.GetByIdAsync(categoryId))
            .ReturnsAsync(category);

        // Act
        Func<Task> act = () => _handler.Handle(new GetByIdDropDownDetailsQuery(categoryId, detailId), CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage($"The DropDownDetails with ID {detailId} was not found.");
    }
}
