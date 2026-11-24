using System;
using AutoMapper;
using FluentAssertions;
using JetBrains.Annotations;
using FinancialSuite.Application.Projects.Commands.Create;
using FinancialSuite.Application.Projects.Commands.Update;
using FinancialSuite.Application.Projects.Dtos;
using FinancialSuite.Domain.Entities;
using Xunit;

namespace FinancialSuite.Application.Tests.Projects.Dtos;

[TestSubject(typeof(ProjectsProfile))]
public class ProjectsProfileTest
{
    private readonly IMapper _mapper;

    public ProjectsProfileTest()
    {
        var configuration = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<ProjectsProfile>();
        });

        _mapper = configuration.CreateMapper();
    }

    [Fact]
    public void CreateMap_For_ProjectDto_ShouldMapPropertiesCorrectly()
    {
        // Arrange 

        var command = new Domain.Entities.Project
        {
            ProjectId = Guid.NewGuid(),
            Name = "Test Project",
            Description = "Test Description",
            DateCreated = new DateTime(2024, 1, 1),
            DateUpdated = new DateTime(2024, 1, 2),
            IsActive = true
        };

        // Act

        var projectDto = _mapper.Map<GetByIdProjectDto>(command);

        // Assert   
        
        projectDto.Should().NotBeNull();
        projectDto.ProjectId.Should().Be(command.ProjectId);
        projectDto.Name.Should().Be(command.Name);
        projectDto.Description.Should().Be(command.Description);
        projectDto.DateCreated.Should().Be(command.DateCreated.ToString("MM-dd-yyyy"));
        projectDto.DateUpdated.Should().Be(command.DateUpdated.ToString("MM-dd-yyyy"));
        projectDto.IsActive.Should().Be(command.IsActive);
    }


    [Fact]
    public void CreateMap_For_CreateProjectDtoToProject_ShouldMapPropertiesCorrectly()
    {
        // Arrange 

        var command = new CreateProjectCommand
        {
            Name = "Test Project",
            Description = "Test Description",
        };

        // Act

        var projectDto = _mapper.Map<Project>(command);

        // Assert   

        projectDto.Should().NotBeNull();
        projectDto.Name.Should().Be(command.Name);
        projectDto.Description.Should().Be(command.Description);
    }

    [Fact]
    public void CreateMap_For_UpdateProjectDtoToProject_ShouldMapPropertiesCorrectly()
    {
        // Arrange 

        var command = new UpdateProjectCommand
        {
            ProjectId = Guid.NewGuid(),
            Name = "Test Project",
            Description = "Test Description",
        };

        // Act

        var projectDto = _mapper.Map<Project>(command);

        // Assert   

        projectDto.Should().NotBeNull();
        projectDto.ProjectId.Should().Be(command.ProjectId);
        projectDto.Name.Should().Be(command.Name);
        projectDto.Description.Should().Be(command.Description);
    }
}