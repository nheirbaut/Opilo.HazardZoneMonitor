using Microsoft.Extensions.Options;
using Opilo.HazardZoneMonitor.Api.Features.HazardZones;

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
}
