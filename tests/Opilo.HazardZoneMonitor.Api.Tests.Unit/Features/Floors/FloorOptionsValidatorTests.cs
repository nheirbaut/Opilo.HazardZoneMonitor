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
        var hazardZone = HazardZoneConfigurationBuilder.Create().WithName(invalidName).Build();
        var floor = new FloorConfiguration(
            "Floor",
            [new PointConfiguration(0, 0), new PointConfiguration(1, 0), new PointConfiguration(0, 1)])
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

    [Fact]
    public void Validate_ShouldReturnFailure_WhenHazardZoneNamesAreNotUniqueWithinFloor()
    {
        // Arrange
        var hazardZone1 = HazardZoneConfigurationBuilder.Create().WithName("Zone A").Build();
        var hazardZone2 = HazardZoneConfigurationBuilder.Create().WithName("Zone A").WithOutline(new PointConfiguration(2, 2), new PointConfiguration(3, 2), new PointConfiguration(2, 3)).Build();
        var floor = new FloorConfiguration(
            "Floor",
            [new PointConfiguration(0, 0), new PointConfiguration(1, 0), new PointConfiguration(0, 1)])
        {
            HazardZones = [hazardZone1, hazardZone2]
        };
        var options = new FloorOptions { Floors = [floor] };

        // Act
        var result = _validator.Validate(string.Empty, options);

        // Assert
        result.Succeeded.Should().BeFalse();
        result.Failures.Should().ContainSingle();
    }

    [Fact]
    public void Validate_ShouldReturnFailure_WhenHazardZoneNamesAreNotUniqueWithinFloorWhenCaseIgnored()
    {
        // Arrange
        var hazardZone1 = HazardZoneConfigurationBuilder.Create().WithName("ZoNe A").Build();
        var hazardZone2 = HazardZoneConfigurationBuilder.Create().WithName("zOnE a").WithOutline(new PointConfiguration(2, 2), new PointConfiguration(3, 2), new PointConfiguration(2, 3)).Build();
        var floor = new FloorConfiguration(
            "Floor",
            [new PointConfiguration(0, 0), new PointConfiguration(1, 0), new PointConfiguration(0, 1)])
        {
            HazardZones = [hazardZone1, hazardZone2]
        };
        var options = new FloorOptions { Floors = [floor] };

        // Act
        var result = _validator.Validate(string.Empty, options);

        // Assert
        result.Succeeded.Should().BeFalse();
        result.Failures.Should().ContainSingle();
    }

    [Fact]
    public void Validate_ShouldReturnFailure_WhenHazardZoneOutlineHasFewerThanThreePoints()
    {
        // Arrange
        var hazardZone = HazardZoneConfigurationBuilder.Create().WithOutline(new PointConfiguration(0, 0), new PointConfiguration(1, 1)).Build();
        var floor = new FloorConfiguration(
            "Floor",
            [new PointConfiguration(0, 0), new PointConfiguration(1, 0), new PointConfiguration(0, 1)])
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

    [Fact]
    public void Validate_ShouldReturnSuccess_WhenHazardZoneOutlineHasExactlyThreePoints()
    {
        // Arrange
        var hazardZone = HazardZoneConfigurationBuilder.BuildSimple();
        var floor = new FloorConfiguration(
            "Floor",
            [new PointConfiguration(0, 0), new PointConfiguration(10, 0), new PointConfiguration(0, 10)])
        {
            HazardZones = [hazardZone]
        };
        var options = new FloorOptions { Floors = [floor] };

        // Act
        var result = _validator.Validate(string.Empty, options);

        // Assert
        result.Succeeded.Should().BeTrue();
        result.Failures.Should().BeNullOrEmpty();
    }

    [Fact]
    public void Validate_ShouldReturnFailure_WhenHazardZoneActivationDurationIsNegative()
    {
        // Arrange
        var hazardZone = HazardZoneConfigurationBuilder.Create().WithActivationDuration(TimeSpan.FromMilliseconds(-1)).Build();
        var floor = new FloorConfiguration(
            "Floor",
            [new PointConfiguration(0, 0), new PointConfiguration(10, 0), new PointConfiguration(0, 10)])
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

    [Fact]
    public void Validate_ShouldReturnFailure_WhenHazardZonePreAlarmDurationIsNegative()
    {
        // Arrange
        var hazardZone = HazardZoneConfigurationBuilder.Create().WithPreAlarmDuration(TimeSpan.FromMilliseconds(-1)).Build();
        var floor = new FloorConfiguration(
            "Floor",
            [new PointConfiguration(0, 0), new PointConfiguration(10, 0), new PointConfiguration(0, 10)])
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

    [Fact]
    public void Validate_ShouldReturnFailure_WhenHazardZoneAllowedNumberOfPersonsIsNegative()
    {
        // Arrange
        var hazardZone = HazardZoneConfigurationBuilder.Create().WithAllowedNumberOfPersons(-1).Build();
        var floor = new FloorConfiguration(
            "Floor",
            [new PointConfiguration(0, 0), new PointConfiguration(10, 0), new PointConfiguration(0, 10)])
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

    [Fact]
    public void Validate_ShouldReturnFailure_WhenHazardZonesOverlapWithinFloor()
    {
        // Arrange
        var hazardZone1 = HazardZoneConfigurationBuilder.Create().WithName("Zone A").WithOutline(new PointConfiguration(0, 0), new PointConfiguration(4, 0), new PointConfiguration(4, 4), new PointConfiguration(0, 4)).Build();
        var hazardZone2 = HazardZoneConfigurationBuilder.Create().WithName("Zone B").WithOutline(new PointConfiguration(2, 2), new PointConfiguration(6, 2), new PointConfiguration(6, 6), new PointConfiguration(2, 6)).Build();
        var floor = new FloorConfiguration(
            "Floor",
            [new PointConfiguration(0, 0), new PointConfiguration(10, 0), new PointConfiguration(10, 10), new PointConfiguration(0, 10)])
        {
            HazardZones = [hazardZone1, hazardZone2]
        };
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
        var floor = new FloorConfiguration(
            "Floor",
            [new PointConfiguration(0, 0), new PointConfiguration(10, 0), new PointConfiguration(10, 10), new PointConfiguration(0, 10)])
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
