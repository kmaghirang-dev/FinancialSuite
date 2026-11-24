using System;
using AutoMapper;
using FluentAssertions;
using JetBrains.Annotations;
using Xunit;
using FinancialSuite.Application.DropDownCategory.Commands.Create;
using FinancialSuite.Application.DropDownCategory.Commands.Toggle;
using FinancialSuite.Application.DropDownCategory.Commands.Update;
using FinancialSuite.Application.DropDownCategory.Dtos;
using FinancialSuite.Application.DropDownDetails.Dtos;
using FinancialSuite.Domain.Entities;

namespace FinancialSuite.Application.Tests.DropDownCategory.Dtos;

[TestSubject(typeof(DropDownCategoryProfile))]
public class DropDownCategoryProfileTest
{
    private readonly IMapper _mapper;

    public DropDownCategoryProfileTest()
    {
        var configuration = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<DropDownCategoryProfile>();
            cfg.AddProfile<DropDownDetailsProfile>();
        });

        _mapper = configuration.CreateMapper();
    }

    [Fact]
    public void CreateMap_ForDropDownCategoryToGetAllDropDownCategoryDto_ShouldMapPropertiesCorrectly()
    {
        // Arrange
        var data = new Domain.Entities.DropDownCategory
        {
            DropDownCategoryId = Guid.NewGuid(),
            Value = "Test Value",
            CreatedBy = "Creator",
            DateCreated = new DateTime(2024, 1, 1),
            UpdatedBy = "Updater",
            DateUpdated = new DateTime(2024, 1, 2),
            IsActive = true
        };

        // Act
        var dto = _mapper.Map<GetAllDropDownCategoryDto>(data);

        // Assert
        dto.Should().NotBeNull();
        dto.DropDownCategoryId.Should().Be(data.DropDownCategoryId);
        dto.Value.Should().Be(data.Value);
        dto.CreatedBy.Should().Be(data.CreatedBy);
        dto.DateCreated.Should().Be(data.DateCreated.ToString("MM-dd-yyyy"));
        dto.UpdatedBy.Should().Be(data.UpdatedBy);
        dto.DateUpdated.Should().Be(data.DateUpdated.ToString("MM-dd-yyyy"));
        dto.IsActive.Should().Be(data.IsActive);
    }

    [Fact]
    public void CreateMap_ForDropDownCategoryToGetByIdDropDownCategoryDto_ShouldMapDetails()
    {
        // Arrange
        var detail = new DropDownDetail
        {
            DropDownDetailId = Guid.NewGuid(),
            Code = "CODE",
            DisplayValue = "Display Value"
        };
        var data = new Domain.Entities.DropDownCategory
        {
            DropDownCategoryId = Guid.NewGuid(),
            Value = "Test Value",
            DropDownDetails = [detail]
        };

        // Act
        var dto = _mapper.Map<GetByIdDropDownCategoryDto>(data);

        // Assert
        dto.Should().NotBeNull();
        dto.DropDownCategoryId.Should().Be(data.DropDownCategoryId);
        dto.Value.Should().Be(data.Value);
        dto.DropDownDetails.Should().ContainSingle();
        dto.DropDownDetails[0].DropDownDetailId.Should().Be(detail.DropDownDetailId);
        dto.DropDownDetails[0].Code.Should().Be(detail.Code);
        dto.DropDownDetails[0].DisplayValue.Should().Be(detail.DisplayValue);
    }

    [Fact]
    public void CreateMap_ForCreateDropDownCategoryCommandToDropDownCategory_ShouldMapPropertiesCorrectly()
    {
        // Arrange
        var command = new CreateDropDownCategoryCommand
        {
            Value = "Test Value",
            CreatedBy = "Creator"
        };

        // Act
        var data = _mapper.Map<Domain.Entities.DropDownCategory>(command);

        // Assert
        data.Should().NotBeNull();
        data.Value.Should().Be(command.Value);
        data.CreatedBy.Should().Be(command.CreatedBy);
        data.IsActive.Should().BeTrue();
        data.DateCreated.Should().BeCloseTo(DateTime.Now, TimeSpan.FromMinutes(1));
        data.DateUpdated.Should().BeCloseTo(DateTime.Now, TimeSpan.FromMinutes(1));
    }

    [Fact]
    public void CreateMap_ForUpdateDropDownCategoryCommandToDropDownCategory_ShouldMapPropertiesCorrectly()
    {
        // Arrange
        var command = new UpdateDropDownCategoryCommand
        {
            DropDownCategoryId = Guid.NewGuid(),
            Value = "Updated Value",
            UpdatedBy = "Updater"
        };

        // Act
        var data = _mapper.Map<Domain.Entities.DropDownCategory>(command);

        // Assert
        data.Should().NotBeNull();
        data.DropDownCategoryId.Should().Be(command.DropDownCategoryId);
        data.Value.Should().Be(command.Value);
        data.UpdatedBy.Should().Be(command.UpdatedBy);
        data.DateUpdated.Should().BeCloseTo(DateTime.Now, TimeSpan.FromMinutes(1));
    }

    [Fact]
    public void CreateMap_ForToggleDropDownCategoryCommandToDropDownCategory_ShouldMapIsActive()
    {
        // Arrange
        var data = new Domain.Entities.DropDownCategory { IsActive = true };
        var command = new ToggleDropDownCategoryCommand
        {
            DropDownCategoryId = Guid.NewGuid(),
            UpdatedBy = "Updater",
            IsActive = false
        };

        // Act
        _mapper.Map(command, data);

        // Assert
        data.IsActive.Should().BeFalse();
        data.UpdatedBy.Should().Be(command.UpdatedBy);
        data.DateUpdated.Should().BeCloseTo(DateTime.Now, TimeSpan.FromMinutes(1));
    }
}
