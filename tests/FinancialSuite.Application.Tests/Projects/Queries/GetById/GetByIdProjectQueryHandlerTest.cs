using System;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using FluentAssertions;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using FinancialSuite.Application.Projects.Dtos;
using FinancialSuite.Application.Projects.Queries.GetById;
using FinancialSuite.Domain.Exceptions;
using FinancialSuite.Domain.Repositories;

namespace FinancialSuite.Application.Tests.Projects.Queries.GetById;

[TestSubject(typeof(GetByIdProjectQueryHandler))]
public class GetByIdProjectQueryHandlerTest
{
    private readonly Mock<IMapper> _mapperMock;
    private readonly Mock<IProjectsRepository> _repositoryMock;
    private readonly GetByIdProjectQueryHandler _handler;

    public GetByIdProjectQueryHandlerTest()
    {
        var loggerMock = new Mock<ILogger<GetByIdProjectQueryHandler>>();
        _mapperMock = new Mock<IMapper>();
        _repositoryMock = new Mock<IProjectsRepository>();
        _handler = new GetByIdProjectQueryHandler(
            loggerMock.Object,
            _mapperMock.Object,
            _repositoryMock.Object);
    }

    [Fact]
    public async Task Handle_ForExistingProject_ReturnsMappedDto()
    {
        // Arrange
        var id = Guid.NewGuid();
        var data = new Domain.Entities.Project { ProjectId = id };
        var dto = new GetByIdProjectDto { ProjectId = id };

        _repositoryMock.Setup(repo => repo.GetByIdAsync(id))
            .ReturnsAsync(data);
        _mapperMock.Setup(mapper => mapper.Map<GetByIdProjectDto>(data))
            .Returns(dto);

        // Act
        var result = await _handler.Handle(new GetByIdProjectQuery(id), CancellationToken.None);

        // Assert
        result.Should().BeSameAs(dto);
    }

    [Fact]
    public async Task Handle_WithNonExistingProject_ShouldThrowNotFoundException()
    {
        // Arrange
        var id = Guid.NewGuid();

        _repositoryMock.Setup(repo => repo.GetByIdAsync(id))
            .ReturnsAsync((Domain.Entities.Project)null);

        // Act
        Func<Task> act = () => _handler.Handle(new GetByIdProjectQuery(id), CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage($"The Projects with ID {id} was not found.");
    }
}
