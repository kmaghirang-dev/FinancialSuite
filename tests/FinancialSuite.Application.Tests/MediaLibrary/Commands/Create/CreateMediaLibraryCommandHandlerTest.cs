using System;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using FluentAssertions;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using FinancialSuite.Application.MediaLibrary.Commands.Create;
using FinancialSuite.Application.Users;
using FinancialSuite.Domain.Repositories;

namespace FinancialSuite.Application.Tests.MediaLibrary.Commands.Create;

[TestSubject(typeof(CreateMediaLibraryCommandHandler))]
public class CreateMediaLibraryCommandHandlerTest
{
    private readonly Mock<IMapper> _mapperMock;
    private readonly Mock<IMediaLibraryRepository> _repositoryMock;
    private readonly Mock<IUserContext> _userContextMock;
    private readonly CreateMediaLibraryCommandHandler _handler;

    public CreateMediaLibraryCommandHandlerTest()
    {
        var loggerMock = new Mock<ILogger<CreateMediaLibraryCommandHandler>>();
        _mapperMock = new Mock<IMapper>();
        _repositoryMock = new Mock<IMediaLibraryRepository>();
        _userContextMock = new Mock<IUserContext>();
        _handler = new CreateMediaLibraryCommandHandler(
            loggerMock.Object,
            _mapperMock.Object,
            _userContextMock.Object,
            _repositoryMock.Object);
    }

    [Fact]
    public async Task Handle_ForValidCommand_ReturnsMediaLibraryId()
    {
        // Arrange
        var command = new CreateMediaLibraryCommand
        {
            CategoryId = Guid.NewGuid(),
            StatusId = Guid.NewGuid(),
            Title = "Test Title",
            Season = 1,
            EpisodeNo = 2,
            Details = "Test Details",
        };
        var data = new Domain.Entities.MediaLibrary();

        _mapperMock.Setup(mapper => mapper.Map<Domain.Entities.MediaLibrary>(command))
            .Returns(data);

        var expectedId = Guid.NewGuid();
        _repositoryMock.Setup(repo => repo.CreateAsync(It.IsAny<Domain.Entities.MediaLibrary>()))
            .ReturnsAsync(expectedId);

        var currentUser = new CurrentUser("OwnerId", "OwnerUserName", ["Owner"], null);
        _userContextMock.Setup(uc => uc.GetCurrentUser()).Returns(currentUser);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().Be(expectedId);
        data.CreatedBy.Should().Be("OwnerUserName");
        data.UpdatedBy.Should().Be("OwnerUserName");
        _repositoryMock.Verify(repo => repo.CreateAsync(data), Times.Once);
    }
}
