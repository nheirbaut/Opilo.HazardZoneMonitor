using Microsoft.Extensions.Options;
using Opilo.HazardZoneMonitor.Api.Features.HazardZones;
using Opilo.HazardZoneMonitor.Api.Shared.Configuration;

namespace Opilo.HazardZoneMonitor.Api.Tests.Unit.Features.HazardZones;

public sealed class HazardZoneOptionsValidatorTests
{
    private readonly HazardZoneOptionsValidator _validator = new();

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
        var options = new HazardZoneOptions { HazardZones = [hazardZone] };

        // Act
        var result = _validator.Validate(string.Empty, options);

        // Assert
        result.Succeeded.Should().BeFalse();
        result.Failures.Should().ContainSingle();
    }
}
