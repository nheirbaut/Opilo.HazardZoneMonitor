using Ardalis.Result;
using Microsoft.Extensions.Options;
using Opilo.HazardZoneMonitor.Api.Features.HazardZones;
using Opilo.HazardZoneMonitor.Api.Features.HazardZones.Configuration;
using Opilo.HazardZoneMonitor.Domain.Shared.Primitives;
using Opilo.HazardZoneMonitor.Domain.Shared.Time;

namespace Opilo.HazardZoneMonitor.Api.Tests.Unit.Features.HazardZones;

public sealed class HazardZoneServiceTests
{
    [Fact]
    public void ActivateHazardZone_ShouldReturnNotFoundResult_WhenNoHazardZonesAreConfigured()
    {
        // Arrange
        var options = Options.Create(new HazardZoneOptions { HazardZones = [] });
        using var hazardZoneService = new HazardZoneService(options, new SystemClock(), new SystemTimerFactory());
        var hazardZoneName = HazardZoneName.From("non-existing-hazardzone");

        // Act
        var result = hazardZoneService.ActivateHazardZone(hazardZoneName);

        // Assert
        result.Status.Should().Be(ResultStatus.NotFound);
    }

    [Fact]
    public void ActivateHazardZone_ShouldReturnNoContentResult_WhenHazardZoneExists()
    {
        // Arrange
        var hazardZoneName = HazardZoneName.From("existing-hazardzone");
        var options = Options.Create(new HazardZoneOptions
        {
            HazardZones =
            [
                new HazardZoneConfiguration(
                    hazardZoneName,
                    [new Coordinate(0, 0), new Coordinate(10, 0), new Coordinate(10, 10), new Coordinate(0, 10)],
                    TimeSpan.FromSeconds(1),
                    TimeSpan.FromSeconds(1))
            ]
        });

        using var hazardZoneService = new HazardZoneService(options, new SystemClock(), new SystemTimerFactory());

        // Act
        var result = hazardZoneService.ActivateHazardZone(hazardZoneName);

        // Assert
        result.Status.Should().Be(ResultStatus.NoContent);
    }
}
