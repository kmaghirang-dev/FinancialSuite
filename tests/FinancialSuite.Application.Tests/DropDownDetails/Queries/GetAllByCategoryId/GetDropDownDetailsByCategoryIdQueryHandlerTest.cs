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
using FinancialSuite.Application.DropDownDetails.Dtos;
using FinancialSuite.Application.DropDownDetails.Queries.GetAllByCategoryId;
using FinancialSuite.Domain.Entities;
using FinancialSuite.Domain.Exceptions;
using FinancialSuite.Domain.Repositories;

namespace FinancialSuite.Application.Tests.DropDownDetails.Queries.GetAllByCategoryId;

[TestSubject(typeof(GetDropDownDetailsByCategoryIdQueryHandler))]
public class GetDropDownDetailsByCategoryIdQueryHandlerTest
{
    private readonly Mock<IMapper> _mapperMock;
    private readonly Mock<IDropDownCategoryRepository> _repositoryMock;
    private readonly GetDropDownDetailsByCategoryIdQueryHandler _handler;

    public GetDropDownDetailsByCategoryIdQueryHandlerTest()
    {
        var loggerMock = new Mock<ILogger<GetDropDownDetailsByCategoryIdQueryHandler>>();
        _mapperMock = new Mock<IMapper>();
        _repositoryMock = new Mock<IDropDownCategoryRepository>();
        _handler = new GetDropDownDetailsByCategoryIdQueryHandler(
            loggerMock.Object,
            _mapperMock.Object,
            _repositoryMock.Object);
    }

    [Fact]
    public async Task Handle_ForExistingDropDownCategory_ReturnsActiveDetailsOnly()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var activeDetail = new DropDownDetail { DropDownDetailId = Guid.NewGuid(), IsActive = true };
        var inactiveDetail = new DropDownDetail { DropDownDetailId = Guid.NewGuid(), IsActive = false };
        var category = new Domain.Entities.DropDownCategory
        {
            DropDownCategoryId = categoryId,
            DropDownDetails = [activeDetail, inactiveDetail]
        };

        _repositoryMock.Setup(repo => repo.GetByIdAsync(categoryId))
            .ReturnsAsync(category);

        var dtos = new List<GetAllDropDownDetailsDto> { new GetAllDropDownDetailsDto { DropDownDetailId = activeDetail.DropDownDetailId } };
        _mapperMock.Setup(mapper => mapper.Map<IEnumerable<GetAllDropDownDetailsDto>>(
                It.Is<List<DropDownDetail>>(details => details.Count == 1 && details.Single() == activeDetail)))
            .Returns(dtos);

        // Act
        var result = await _handler.Handle(new GetDropDownDetailsByCategoryIdQuery(categoryId), CancellationToken.None);

        // Assert
        result.Should().BeSameAs(dtos);
    }

    [Fact]
    public async Task Handle_WithNonExistingDropDownCategory_ShouldThrowNotFoundException()
    {
        // Arrange
        var categoryId = Guid.NewGuid();

        _repositoryMock.Setup(repo => repo.GetByIdAsync(categoryId))
            .ReturnsAsync((Domain.Entities.DropDownCategory)null);

        // Act
        Func<Task> act = () => _handler.Handle(new GetDropDownDetailsByCategoryIdQuery(categoryId), CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage($"The DropDownCategory with ID {categoryId} was not found.");
    }
}
