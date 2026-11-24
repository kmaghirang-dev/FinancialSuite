using System;
using AutoMapper;
using FluentAssertions;
using JetBrains.Annotations;
using Xunit;
using FinancialSuite.Application.DropDownDetails.Commands.Create;
using FinancialSuite.Application.DropDownDetails.Commands.Toggle;
using FinancialSuite.Application.DropDownDetails.Dtos;
using FinancialSuite.Domain.Entities;

namespace FinancialSuite.Application.Tests.DropDownDetails.Dtos;

[TestSubject(typeof(DropDownDetailsProfile))]
public class DropDownDetailsProfileTest
{
    private readonly IMapper _mapper;

    public DropDownDetailsProfileTest()
    {
        var configuration = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<DropDownDetailsProfile>();
        });

        _mapper = configuration.CreateMapper();
    }

    [Fact]
    public void CreateMap_ForDropDownDetailToGetAllDropDownDetailsDto_ShouldMapPropertiesCorrectly()
    {
        // Arrange
        var data = new DropDownDetail
        {
            DropDownDetailId = Guid.NewGuid(),
            DropDownCategoryId = Guid.NewGuid(),
            Code = "CODE",
            DisplayValue = "Display Value",
            CreatedBy = "Creator",
            DateCreated = new DateTime(2024, 1, 1),
            UpdatedBy = "Updater",
            DateUpdated = new DateTime(2024, 1, 2),
            IsActive = true
        };

        // Act
        var dto = _mapper.Map<GetAllDropDownDetailsDto>(data);

        // Assert
        dto.Should().NotBeNull();
        dto.DropDownDetailId.Should().Be(data.DropDownDetailId);
        dto.DropDownCategoryId.Should().Be(data.DropDownCategoryId);
        dto.Code.Should().Be(data.Code);
        dto.DisplayValue.Should().Be(data.DisplayValue);
        dto.CreatedBy.Should().Be(data.CreatedBy);
        dto.DateCreated.Should().Be(data.DateCreated.ToString("MM-dd-yyyy"));
        dto.UpdatedBy.Should().Be(data.UpdatedBy);
        dto.DateUpdated.Should().Be(data.DateUpdated.ToString("MM-dd-yyyy"));
        dto.IsActive.Should().Be(data.IsActive);
    }

    [Fact]
    public void CreateMap_ForDropDownDetailToGetByIdDropDownDetailsDto_ShouldMapPropertiesCorrectly()
    {
        // Arrange
        var data = new DropDownDetail
        {
            DropDownDetailId = Guid.NewGuid(),
            Code = "CODE",
            DisplayValue = "Display Value",
            CreatedBy = "Creator"
        };

        // Act
        var dto = _mapper.Map<GetByIdDropDownDetailsDto>(data);

        // Assert
        dto.Should().NotBeNull();
        dto.DropDownDetailId.Should().Be(data.DropDownDetailId);
        dto.Code.Should().Be(data.Code);
        dto.DisplayValue.Should().Be(data.DisplayValue);
        dto.CreatedBy.Should().Be(data.CreatedBy);
    }

    [Fact]
    public void CreateMap_ForCreateDropDownDetailsCommandToDropDownDetail_ShouldMapPropertiesCorrectly()
    {
        // Arrange
        var command = new CreateDropDownDetailsCommand
        {
            DropDownCategoryId = Guid.NewGuid(),
            Code = "CODE",
            DisplayValue = "Display Value",
            CreatedBy = "Creator"
        };

        // Act
        var data = _mapper.Map<DropDownDetail>(command);

        // Assert
        data.Should().NotBeNull();
        data.DropDownCategoryId.Should().Be(command.DropDownCategoryId);
        data.Code.Should().Be(command.Code);
        data.DisplayValue.Should().Be(command.DisplayValue);
        data.CreatedBy.Should().Be(command.CreatedBy);
        data.IsActive.Should().BeTrue();
        data.DateCreated.Should().BeCloseTo(DateTime.Now, TimeSpan.FromMinutes(1));
        data.DateUpdated.Should().BeCloseTo(DateTime.Now, TimeSpan.FromMinutes(1));
    }

    [Fact]
    public void CreateMap_ForToggleDropDownDetailsCommandToDropDownDetail_ShouldMapIsActive()
    {
        // Arrange
        var data = new DropDownDetail { IsActive = true };
        var command = new ToggleDropDownDetailsCommand
        {
            DropDownCategoryId = Guid.NewGuid(),
            DropDownDetailId = Guid.NewGuid(),
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
