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
using FinancialSuite.Application.DropDownCategory.Dtos;
using FinancialSuite.Application.DropDownCategory.Queries.GetAll;
using FinancialSuite.Domain.Repositories;

namespace FinancialSuite.Application.Tests.DropDownCategory.Queries.GetAll;

[TestSubject(typeof(GetAllDropDownCategoryQueryHandler))]
public class GetAllDropDownCategoryQueryHandlerTest
{
    private readonly Mock<IMapper> _mapperMock;
    private readonly Mock<IDropDownCategoryRepository> _repositoryMock;
    private readonly GetAllDropDownCategoryQueryHandler _handler;

    public GetAllDropDownCategoryQueryHandlerTest()
    {
        var loggerMock = new Mock<ILogger<GetAllDropDownCategoryQueryHandler>>();
        _mapperMock = new Mock<IMapper>();
        _repositoryMock = new Mock<IDropDownCategoryRepository>();
        _handler = new GetAllDropDownCategoryQueryHandler(
            loggerMock.Object,
            _mapperMock.Object,
            _repositoryMock.Object);
    }

    [Fact]
    public async Task Handle_ForActiveAndInactiveRecords_ReturnsActiveRecordsOnly()
    {
        // Arrange
        var active = new Domain.Entities.DropDownCategory { DropDownCategoryId = Guid.NewGuid(), IsActive = true };
        var inactive = new Domain.Entities.DropDownCategory { DropDownCategoryId = Guid.NewGuid(), IsActive = false };

        _repositoryMock.Setup(repo => repo.GetAllAsync())
            .ReturnsAsync(new List<Domain.Entities.DropDownCategory> { active, inactive });

        var dtos = new List<GetAllDropDownCategoryDto> { new GetAllDropDownCategoryDto { DropDownCategoryId = active.DropDownCategoryId } };
        _mapperMock.Setup(mapper => mapper.Map<IEnumerable<GetAllDropDownCategoryDto>>(
                It.Is<IEnumerable<Domain.Entities.DropDownCategory>>(data => data.Count() == 1 && data.Single() == active)))
            .Returns(dtos);

        // Act
        var result = await _handler.Handle(new GetAllDropDownCategoryQuery(), CancellationToken.None);

        // Assert
        result.Should().BeSameAs(dtos);
    }
}
