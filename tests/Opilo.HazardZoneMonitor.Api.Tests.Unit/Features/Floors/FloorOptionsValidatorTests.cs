using Microsoft.Extensions.Options;
using Opilo.HazardZoneMonitor.Api.Features.Floors;
using Opilo.HazardZoneMonitor.Api.Features.HazardZones;
using Opilo.HazardZoneMonitor.Api.Shared.Configuration;

namespace Opilo.HazardZoneMonitor.Api.Tests.Unit.Features.Floors;

public sealed class FloorOptionsValidatorTests
{
    private readonly FloorOptionsValidator _validator = new();

    [Fact]
    public void Validate_ShouldReturnSuccess_WhenNoFloorsAreConfigured()
    {
        // Arrange
        var options = new FloorOptions { Floors = [] };

        // Act
        var result = _validator.Validate(string.Empty, options);

        // Assert
        result.Succeeded.Should().BeTrue();
        result.Failures.Should().BeNullOrEmpty();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_ShouldReturnFailure_WhenFloorNameIsEmptyOrWhitespace(string invalidName)
    {
        // Arrange
        var floor = new FloorConfiguration(invalidName, []);
        var options = new FloorOptions { Floors = [floor] };

        // Act
        var result = _validator.Validate(string.Empty, options);

        // Assert
        result.Succeeded.Should().BeFalse();
        result.Failures.Should().ContainSingle();
    }

    [Fact]
    public void Validate_ShouldReturnFailure_WhenFloorNamesAreNotUnique()
    {
        // Arrange
        var floor1 = new FloorConfiguration("Floor", []);
        var floor2 = new FloorConfiguration("Floor", []);
        var options = new FloorOptions { Floors = [floor1, floor2] };

        // Act
        var result = _validator.Validate(string.Empty, options);

        // Assert
        result.Succeeded.Should().BeFalse();
        result.Failures.Should().ContainSingle();
    }

    [Fact]
    public void Validate_ShouldReturnFailure_WhenFloorNamesAreNotUniqueWhenCaseIgnored()
    {
        // Arrange
        var floor1 = new FloorConfiguration("FlOoR", []);
        var floor2 = new FloorConfiguration("fLoOr", []);
        var options = new FloorOptions { Floors = [floor1, floor2] };

        // Act
        var result = _validator.Validate(string.Empty, options);

        // Assert
        result.Succeeded.Should().BeFalse();
        result.Failures.Should().ContainSingle();
    }

    [Fact]
    public void Validate_ShouldReturnFailure_WhenFloorOutlineHasFewerThanThreePoints()
    {
        // Arrange
        var floor = new FloorConfiguration("Floor", [new PointConfiguration(0, 0), new PointConfiguration(1, 1)]);
        var options = new FloorOptions { Floors = [floor] };

        // Act
        var result = _validator.Validate(string.Empty, options);

        // Assert
        result.Succeeded.Should().BeFalse();
        result.Failures.Should().ContainSingle();
    }

    [Fact]
    public void Validate_ShouldReturnSuccess_WhenFloorOutlineHasExactlyThreePoints()
    {
        // Arrange
        var floor = new FloorConfiguration("Floor", [
            new PointConfiguration(0, 0),
            new PointConfiguration(1, 0),
            new PointConfiguration(0, 1)
        ]);
        var options = new FloorOptions { Floors = [floor] };

        // Act
        var result = _validator.Validate(string.Empty, options);

        // Assert
        result.Succeeded.Should().BeTrue();
        result.Failures.Should().BeNullOrEmpty();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_ShouldReturnFailure_WhenHazardZoneNameIsEmptyOrWhitespace(string invalidName)
    {
        // Arrange
        var hazardZone = new HazardZoneConfiguration(
            invalidName,
            [new PointConfiguration(0, 0), new PointConfiguration(1, 0), new PointConfiguration(0, 1)],
            TimeSpan.Zero,
            TimeSpan.Zero);
        var floor = new FloorConfiguration("Floor", [new PointConfiguration(0, 0), new PointConfiguration(1, 0), new PointConfiguration(0, 1)])
        {
            HazardZones = [hazardZone]
        };
        var options = new FloorOptions { Floors = [floor] };

        // Act
        var result = _validator.Validate(string.Empty, options);

        // Assert
        result.Succeeded.Should().BeFalse();
        result.Failures.Should().ContainSingle();
    }
}
