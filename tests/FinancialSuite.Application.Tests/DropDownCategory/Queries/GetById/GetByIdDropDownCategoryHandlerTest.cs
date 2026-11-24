using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using FluentAssertions;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using FinancialSuite.Application.DropDownCategory.Dtos;
using FinancialSuite.Application.DropDownCategory.Queries.GetById;
using FinancialSuite.Domain.Entities;
using FinancialSuite.Domain.Repositories;

namespace FinancialSuite.Application.Tests.DropDownCategory.Queries.GetById;

[TestSubject(typeof(GetByIdDropDownCategoryHandler))]
public class GetByIdDropDownCategoryHandlerTest
{
    private readonly Mock<IMapper> _mapperMock;
    private readonly Mock<IDropDownCategoryRepository> _repositoryMock;
    private readonly GetByIdDropDownCategoryHandler _handler;

    public GetByIdDropDownCategoryHandlerTest()
    {
        var loggerMock = new Mock<ILogger<GetByIdDropDownCategoryHandler>>();
        _mapperMock = new Mock<IMapper>();
        _repositoryMock = new Mock<IDropDownCategoryRepository>();
        _handler = new GetByIdDropDownCategoryHandler(
            loggerMock.Object,
            _mapperMock.Object,
            _repositoryMock.Object);
    }

    [Fact]
    public async Task Handle_ForExistingDropDownCategory_ReturnsMappedDtoWithActiveDetailsOnly()
    {
        // Arrange
        var id = Guid.NewGuid();
        var activeDetail = new DropDownDetail { DropDownDetailId = Guid.NewGuid(), IsActive = true };
        var inactiveDetail = new DropDownDetail { DropDownDetailId = Guid.NewGuid(), IsActive = false };
        var data = new Domain.Entities.DropDownCategory
        {
            DropDownCategoryId = id,
            DropDownDetails = [activeDetail, inactiveDetail]
        };
        var dto = new GetByIdDropDownCategoryDto { DropDownCategoryId = id };

        _repositoryMock.Setup(repo => repo.GetByIdAsync(id))
            .ReturnsAsync(data);
        _mapperMock.Setup(mapper => mapper.Map<GetByIdDropDownCategoryDto>(data))
            .Returns(dto);

        // Act
        var result = await _handler.Handle(new GetByIdDropDownCategoryQuery(id), CancellationToken.None);

        // Assert
        result.Should().BeSameAs(dto);
        data.DropDownDetails.Should().BeEquivalentTo(new List<DropDownDetail> { activeDetail });
    }
}
