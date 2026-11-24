using System;
using AutoMapper;
using FluentAssertions;
using JetBrains.Annotations;
using Xunit;
using FinancialSuite.Application.MediaLibrary.Commands.Create;
using FinancialSuite.Application.MediaLibrary.Commands.Toggle;
using FinancialSuite.Application.MediaLibrary.Commands.Update;
using FinancialSuite.Application.MediaLibrary.Dtos;

namespace FinancialSuite.Application.Tests.MediaLibrary.Dtos;

[TestSubject(typeof(MediaLibraryProfile))]
public class MediaLibraryProfileTest
{
    private readonly IMapper _mapper;

    public MediaLibraryProfileTest()
    {
        var configuration = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<MediaLibraryProfile>();
        });

        _mapper = configuration.CreateMapper();
    }

    [Fact]
    public void CreateMap_ForMediaLibraryToGetByIdMediaLibraryDto_ShouldMapPropertiesCorrectly()
    {
        // Arrange
        var data = new Domain.Entities.MediaLibrary
        {
            MediaId = Guid.NewGuid(),
            CategoryId = Guid.NewGuid(),
            StatusId = Guid.NewGuid(),
            Title = "Test Title",
            Season = 1,
            EpisodeNo = 2,
            Details = "Test Details",
            CreatedBy = "Creator",
            DateCreated = new DateTime(2024, 1, 1),
            UpdatedBy = "Updater",
            DateUpdated = new DateTime(2024, 1, 2),
            IsActive = true
        };

        // Act
        var dto = _mapper.Map<GetByIdMediaLibraryDto>(data);

        // Assert
        dto.Should().NotBeNull();
        dto.MediaId.Should().Be(data.MediaId);
        dto.CategoryId.Should().Be(data.CategoryId);
        dto.StatusId.Should().Be(data.StatusId);
        dto.Title.Should().Be(data.Title);
        dto.Season.Should().Be(data.Season);
        dto.EpisodeNo.Should().Be(data.EpisodeNo);
        dto.Details.Should().Be(data.Details);
        dto.CreatedBy.Should().Be(data.CreatedBy);
        dto.DateCreated.Should().Be(data.DateCreated.ToString("MM-dd-yyyy"));
        dto.UpdatedBy.Should().Be(data.UpdatedBy);
        dto.DateUpdated.Should().Be(data.DateUpdated.ToString("MM-dd-yyyy"));
        dto.IsActive.Should().Be(data.IsActive);
    }

    [Fact]
    public void CreateMap_ForMediaLibraryToGetAllMediaLibraryDto_ShouldMapPropertiesCorrectly()
    {
        // Arrange
        var data = new Domain.Entities.MediaLibrary
        {
            MediaId = Guid.NewGuid(),
            CategoryId = Guid.NewGuid(),
            StatusId = Guid.NewGuid(),
            Title = "Test Title",
            Season = 1,
            EpisodeNo = 2,
            Details = "Test Details",
            CreatedBy = "Creator",
            DateCreated = new DateTime(2024, 1, 1),
            UpdatedBy = "Updater",
            DateUpdated = new DateTime(2024, 1, 2),
            IsActive = true
        };

        // Act
        var dto = _mapper.Map<GetAllMediaLibraryDto>(data);

        // Assert
        dto.Should().NotBeNull();
        dto.MediaId.Should().Be(data.MediaId);
        dto.CategoryId.Should().Be(data.CategoryId);
        dto.StatusId.Should().Be(data.StatusId);
        dto.Title.Should().Be(data.Title);
        dto.Season.Should().Be(data.Season);
        dto.EpisodeNo.Should().Be(data.EpisodeNo);
        dto.Details.Should().Be(data.Details);
        dto.CreatedBy.Should().Be(data.CreatedBy);
        dto.DateCreated.Should().Be(data.DateCreated.ToString("MM-dd-yyyy"));
        dto.UpdatedBy.Should().Be(data.UpdatedBy);
        dto.DateUpdated.Should().Be(data.DateUpdated.ToString("MM-dd-yyyy"));
        dto.IsActive.Should().Be(data.IsActive);
    }

    [Fact]
    public void CreateMap_ForCreateMediaLibraryCommandToMediaLibrary_ShouldMapPropertiesCorrectly()
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

        // Act
        var data = _mapper.Map<Domain.Entities.MediaLibrary>(command);

        // Assert
        data.Should().NotBeNull();
        data.CategoryId.Should().Be(command.CategoryId);
        data.StatusId.Should().Be(command.StatusId);
        data.Title.Should().Be(command.Title);
        data.Season.Should().Be(command.Season);
        data.EpisodeNo.Should().Be(command.EpisodeNo);
        data.Details.Should().Be(command.Details);
        data.IsActive.Should().BeTrue();
        data.DateCreated.Should().BeCloseTo(DateTime.Now, TimeSpan.FromMinutes(1));
        data.DateUpdated.Should().BeCloseTo(DateTime.Now, TimeSpan.FromMinutes(1));
    }

    [Fact]
    public void CreateMap_ForUpdateMediaLibraryCommandToMediaLibrary_ShouldMapPropertiesCorrectly()
    {
        // Arrange
        var command = new UpdateMediaLibraryCommand
        {
            MediaId = Guid.NewGuid(),
            CategoryId = Guid.NewGuid(),
            StatusId = Guid.NewGuid(),
            Title = "Test Title",
            Season = 1,
            EpisodeNo = 2,
            Details = "Test Details",
        };

        // Act
        var data = _mapper.Map<Domain.Entities.MediaLibrary>(command);

        // Assert
        data.Should().NotBeNull();
        data.MediaId.Should().Be(command.MediaId);
        data.CategoryId.Should().Be(command.CategoryId);
        data.StatusId.Should().Be(command.StatusId);
        data.Title.Should().Be(command.Title);
        data.Season.Should().Be(command.Season);
        data.EpisodeNo.Should().Be(command.EpisodeNo);
        data.Details.Should().Be(command.Details);
        data.DateUpdated.Should().BeCloseTo(DateTime.Now, TimeSpan.FromMinutes(1));
    }

    [Fact]
    public void CreateMap_ForToggleMediaLibraryCommandToMediaLibrary_ShouldMapIsActive()
    {
        // Arrange
        var data = new Domain.Entities.MediaLibrary { IsActive = true };
        var command = new ToggleMediaLibraryCommand { MediaId = Guid.NewGuid(), IsActive = false };

        // Act
        _mapper.Map(command, data);

        // Assert
        data.IsActive.Should().BeFalse();
        data.DateUpdated.Should().BeCloseTo(DateTime.Now, TimeSpan.FromMinutes(1));
    }
}
