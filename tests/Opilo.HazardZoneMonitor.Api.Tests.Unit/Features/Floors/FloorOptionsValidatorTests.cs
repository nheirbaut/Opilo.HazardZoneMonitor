using Opilo.HazardZoneMonitor.Api.Features.Floors;
using Opilo.HazardZoneMonitor.Domain.Shared.Primitives;
using Opilo.HazardZoneMonitor.Tests.Common.TestUtilities.Builders;

namespace Opilo.HazardZoneMonitor.Api.Tests.Unit.Features.Floors;

public sealed class FloorOptionsValidatorTests
{
    private readonly FloorOptionsValidator _validator = new();

    [Fact]
    public void Constructor_ShouldAcceptFloorName_WhenNameIsValid()
    {
        // Arrange
        var floorName = FloorName.From("TestFloor");

        // Act
        var floor = new FloorConfiguration(floorName, FloorConfigurationBuilder.DefaultOutline);

        // Assert
        floor.Name.Should().Be(floorName);
    }

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
    public void Validate_ShouldReturnSuccess_WhenFloorNamesDifferOnlyByCase()
    {
        // Arrange
        var floor1 = FloorConfigurationBuilder.Create().WithName("FlOoR").WithOutline(new Coordinate(0, 0), new Coordinate(1, 0), new Coordinate(0, 1)).Build();
        var floor2 = FloorConfigurationBuilder.Create().WithName("fLoOr").WithOutline(new Coordinate(2, 2), new Coordinate(3, 2), new Coordinate(2, 3)).Build();
        var options = new FloorOptions { Floors = [floor1, floor2] };

        // Act
        var result = _validator.Validate(string.Empty, options);

        // Assert
        result.Succeeded.Should().BeTrue();
        result.Failures.Should().BeNullOrEmpty();
    }

    [Fact]
    public void Validate_ShouldReturnFailure_WhenFloorOutlineHasFewerThanThreePoints()
    {
        // Arrange
        var floor = FloorConfigurationBuilder.Create().WithOutline(new Coordinate(0, 0), new Coordinate(1, 1)).Build();
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
            new Coordinate(0, 0),
            new Coordinate(1, 0),
            new Coordinate(0, 1)).Build();
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
        var hazardZone1 = HazardZoneConfigurationBuilder.Create().WithName("Zone A").WithOutline(new Coordinate(0, 0), new Coordinate(4, 0), new Coordinate(4, 4), new Coordinate(0, 4)).Build();
        var hazardZone2 = HazardZoneConfigurationBuilder.Create().WithName("Zone B").WithOutline(new Coordinate(2, 2), new Coordinate(6, 2), new Coordinate(6, 6), new Coordinate(2, 6)).Build();
        var floor = FloorConfigurationBuilder.Create().WithOutline(
            new Coordinate(0, 0),
            new Coordinate(10, 0),
            new Coordinate(10, 10),
            new Coordinate(0, 10)).WithHazardZones(hazardZone1, hazardZone2).Build();
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
        var hazardZone = HazardZoneConfigurationBuilder.Create().WithOutline(new Coordinate(15, 15), new Coordinate(20, 15), new Coordinate(20, 20), new Coordinate(15, 20)).Build();
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
        var floor = new FloorConfiguration(FloorName.From("Floor"), null!);
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
        var floor = new FloorConfiguration(FloorName.From("Floor"), FloorConfigurationBuilder.DefaultOutline)
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
