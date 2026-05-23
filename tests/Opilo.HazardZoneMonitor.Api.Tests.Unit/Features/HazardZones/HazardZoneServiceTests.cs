using Ardalis.Result;
using Opilo.HazardZoneMonitor.Api.Features.HazardZones;
using Opilo.HazardZoneMonitor.Domain.Shared.Primitives;

namespace Opilo.HazardZoneMonitor.Api.Tests.Unit.Features.HazardZones;

public sealed class HazardZoneServiceTests
{
    [Fact]
    public void ActivateHazardZone_ShouldReturnNotFoundResult_WhenNoHazardZonesAreConfigured()
    {
        // Arrange
        var hazardZoneService = new HazardZoneService();
        var hazardZoneName = HazardZoneName.From("non-existing-hazardzone");

        // Act
        var result = hazardZoneService.ActivateHazardZone(hazardZoneName);

        // Assert
        result.Status.Should().Be(ResultStatus.NotFound);
    }
}
