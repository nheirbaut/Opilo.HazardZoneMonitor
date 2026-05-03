using Opilo.HazardZoneMonitor.Api.Features.Floors;
using Opilo.HazardZoneMonitor.Api.Shared.Configuration;
using Opilo.HazardZoneMonitor.Tests.Common.TestUtilities.Builders;

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
        var floor = FloorConfigurationBuilder.Create().WithName(invalidName).WithOutline().Build();
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
        var floor1 = FloorConfigurationBuilder.Create().WithOutline().Build();
        var floor2 = FloorConfigurationBuilder.Create().WithOutline().Build();
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
        var floor1 = FloorConfigurationBuilder.Create().WithName("FlOoR").WithOutline().Build();
        var floor2 = FloorConfigurationBuilder.Create().WithName("fLoOr").WithOutline().Build();
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
        var floor = FloorConfigurationBuilder.Create().WithOutline(new PointConfiguration(0, 0), new PointConfiguration(1, 1)).Build();
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
        var floor = FloorConfigurationBuilder.Create().WithOutline(
            new PointConfiguration(0, 0),
            new PointConfiguration(1, 0),
            new PointConfiguration(0, 1)).Build();
        var options = new FloorOptions { Floors = [floor] };

        // Act
        var result = _validator.Validate(string.Empty, options);

        // Assert
        result.Succeeded.Should().BeTrue();
        result.Failures.Should().BeNullOrEmpty();
    }

    [Fact]
    public void Validate_ShouldReturnFailure_WhenHazardZonesOverlapWithinFloor()
    {
        // Arrange
        var hazardZone1 = HazardZoneConfigurationBuilder.Create().WithName("Zone A").WithOutline(new PointConfiguration(0, 0), new PointConfiguration(4, 0), new PointConfiguration(4, 4), new PointConfiguration(0, 4)).Build();
        var hazardZone2 = HazardZoneConfigurationBuilder.Create().WithName("Zone B").WithOutline(new PointConfiguration(2, 2), new PointConfiguration(6, 2), new PointConfiguration(6, 6), new PointConfiguration(2, 6)).Build();
        var floor = FloorConfigurationBuilder.Create().WithOutline(
            new PointConfiguration(0, 0),
            new PointConfiguration(10, 0),
            new PointConfiguration(10, 10),
            new PointConfiguration(0, 10)).WithHazardZones(hazardZone1, hazardZone2).Build();
        var options = new FloorOptions { Floors = [floor] };

        // Act
        var result = _validator.Validate(string.Empty, options);

        // Assert
        result.Succeeded.Should().BeFalse();
        result.Failures.Should().ContainSingle();
    }

    [Fact]
    public void Validate_ShouldReturnFailure_WhenHazardZoneIsOutsideFloorOutline()
    {
        // Arrange
        var hazardZone = HazardZoneConfigurationBuilder.Create().WithOutline(new PointConfiguration(15, 15), new PointConfiguration(20, 15), new PointConfiguration(20, 20), new PointConfiguration(15, 20)).Build();
        var floor = FloorConfigurationBuilder.BuildSimple();
        floor = floor with { HazardZones = [hazardZone] };
        var options = new FloorOptions { Floors = [floor] };

        // Act
        var result = _validator.Validate(string.Empty, options);

        // Assert
        result.Succeeded.Should().BeFalse();
        result.Failures.Should().ContainSingle();
    }

    [Fact]
    public void Validate_ShouldReturnFailure_WhenFloorsIsNull()
    {
        // Arrange
        var options = new FloorOptions { Floors = null! };

        // Act
        var result = _validator.Validate(string.Empty, options);

        // Assert
        result.Succeeded.Should().BeFalse();
        result.Failures.Should().ContainSingle();
    }

    [Fact]
    public void Validate_ShouldReturnFailure_WhenFloorOutlineIsNull()
    {
        // Arrange
        var floor = new FloorConfiguration("Floor", null!);
        var options = new FloorOptions { Floors = [floor] };

        // Act
        var result = _validator.Validate(string.Empty, options);

        // Assert
        result.Succeeded.Should().BeFalse();
        result.Failures.Should().ContainSingle();
    }

    [Fact]
    public void Validate_ShouldReturnFailure_WhenHazardZonesIsNull()
    {
        // Arrange
        var floor = new FloorConfiguration("Floor", FloorConfigurationBuilder.DefaultOutline)
        {
            HazardZones = null!
        };
        var options = new FloorOptions { Floors = [floor] };

        // Act
        var result = _validator.Validate(string.Empty, options);

        // Assert
        result.Succeeded.Should().BeFalse();
        result.Failures.Should().ContainSingle();
    }
}
