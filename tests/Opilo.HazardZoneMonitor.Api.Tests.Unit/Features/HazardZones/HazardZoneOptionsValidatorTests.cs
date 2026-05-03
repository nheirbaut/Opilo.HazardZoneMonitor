using Opilo.HazardZoneMonitor.Api.Features.HazardZones;
using Opilo.HazardZoneMonitor.Api.Shared.Configuration;
using Opilo.HazardZoneMonitor.Tests.Common.TestUtilities.Builders;

namespace Opilo.HazardZoneMonitor.Api.Tests.Unit.Features.HazardZones;

public sealed class HazardZoneOptionsValidatorTests
{
    private readonly HazardZoneOptionsValidator _validator = new();

    [Fact]
    public void Validate_ShouldReturnFailure_WhenHazardZonesIsNull()
    {
        // Arrange
        var options = new HazardZoneOptions { HazardZones = null! };

        // Act
        var result = _validator.Validate(string.Empty, options);

        // Assert
        result.Succeeded.Should().BeFalse();
        result.Failures.Should().ContainSingle();
    }

    [Fact]
    public void Validate_ShouldReturnSuccess_WhenNoHazardZonesAreConfigured()
    {
        // Arrange
        var options = new HazardZoneOptions { HazardZones = [] };

        // Act
        var result = _validator.Validate(string.Empty, options);

        // Assert
        result.Succeeded.Should().BeTrue();
        result.Failures.Should().BeNullOrEmpty();
    }

    [Fact]
    public void Validate_ShouldReturnFailure_WhenHazardZoneOutlineIsNull()
    {
        // Arrange
        var hazardZone = new HazardZoneConfiguration("Zone", null!, TimeSpan.Zero, TimeSpan.Zero);
        var options = new HazardZoneOptions { HazardZones = [hazardZone] };

        // Act
        var result = _validator.Validate(string.Empty, options);

        // Assert
        result.Succeeded.Should().BeFalse();
        result.Failures.Should().ContainSingle();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_ShouldReturnFailure_WhenHazardZoneNameIsEmptyOrWhitespace(string invalidName)
    {
        // Arrange
        var hazardZone = HazardZoneConfigurationBuilder.Create().WithName(invalidName).Build();
        var options = new HazardZoneOptions { HazardZones = [hazardZone] };

        // Act
        var result = _validator.Validate(string.Empty, options);

        // Assert
        result.Succeeded.Should().BeFalse();
        result.Failures.Should().ContainSingle();
    }

    [Fact]
    public void Validate_ShouldReturnFailure_WhenHazardZoneNamesAreNotUnique()
    {
        // Arrange
        var hazardZone1 = HazardZoneConfigurationBuilder.Create().WithName("Zone A").Build();
        var hazardZone2 = HazardZoneConfigurationBuilder.Create().WithName("Zone A").WithOutline(new PointConfiguration(2, 2), new PointConfiguration(3, 2), new PointConfiguration(2, 3)).Build();
        var options = new HazardZoneOptions { HazardZones = [hazardZone1, hazardZone2] };

        // Act
        var result = _validator.Validate(string.Empty, options);

        // Assert
        result.Succeeded.Should().BeFalse();
        result.Failures.Should().ContainSingle();
    }

    [Fact]
    public void Validate_ShouldReturnFailure_WhenHazardZoneNamesAreNotUniqueWhenCaseIgnored()
    {
        // Arrange
        var hazardZone1 = HazardZoneConfigurationBuilder.Create().WithName("ZoNe A").Build();
        var hazardZone2 = HazardZoneConfigurationBuilder.Create().WithName("zOnE a").WithOutline(new PointConfiguration(2, 2), new PointConfiguration(3, 2), new PointConfiguration(2, 3)).Build();
        var options = new HazardZoneOptions { HazardZones = [hazardZone1, hazardZone2] };

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
        var options = new HazardZoneOptions { HazardZones = [hazardZone] };

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
        var options = new HazardZoneOptions { HazardZones = [hazardZone] };

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
        var options = new HazardZoneOptions { HazardZones = [hazardZone] };

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
        var options = new HazardZoneOptions { HazardZones = [hazardZone] };

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
        var options = new HazardZoneOptions { HazardZones = [hazardZone] };

        // Act
        var result = _validator.Validate(string.Empty, options);

        // Assert
        result.Succeeded.Should().BeFalse();
        result.Failures.Should().ContainSingle();
    }
}
