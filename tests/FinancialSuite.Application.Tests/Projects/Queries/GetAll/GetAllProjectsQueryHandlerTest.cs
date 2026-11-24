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
using FinancialSuite.Application.Projects.Dtos;
using FinancialSuite.Application.Projects.Queries.GetAll;
using FinancialSuite.Domain.Repositories;

namespace FinancialSuite.Application.Tests.Projects.Queries.GetAll;

[TestSubject(typeof(GetAllProjectsQueryHandler))]
public class GetAllProjectsQueryHandlerTest
{
    private readonly Mock<IMapper> _mapperMock;
    private readonly Mock<IProjectsRepository> _repositoryMock;
    private readonly GetAllProjectsQueryHandler _handler;

    public GetAllProjectsQueryHandlerTest()
    {
        var loggerMock = new Mock<ILogger<GetAllProjectsQueryHandler>>();
        _mapperMock = new Mock<IMapper>();
        _repositoryMock = new Mock<IProjectsRepository>();
        _handler = new GetAllProjectsQueryHandler(
            loggerMock.Object,
            _mapperMock.Object,
            _repositoryMock.Object);
    }

    [Fact]
    public async Task Handle_ForActiveAndInactiveRecords_ReturnsActiveRecordsOnly()
    {
        // Arrange
        var active = new Domain.Entities.Project { ProjectId = Guid.NewGuid(), IsActive = true };
        var inactive = new Domain.Entities.Project { ProjectId = Guid.NewGuid(), IsActive = false };

        _repositoryMock.Setup(repo => repo.GetAllAsync())
            .ReturnsAsync(new List<Domain.Entities.Project> { active, inactive });

        var dtos = new List<GetAllProjectsDto> { new GetAllProjectsDto { ProjectId = active.ProjectId } };
        _mapperMock.Setup(mapper => mapper.Map<IEnumerable<GetAllProjectsDto>>(
                It.Is<IEnumerable<Domain.Entities.Project>>(data => data.Count() == 1 && data.Single() == active)))
            .Returns(dtos);

        // Act
        var result = await _handler.Handle(new GetAllProjectsQuery(), CancellationToken.None);

        // Assert
        result.Should().BeSameAs(dtos);
    }
}
