using Ardalis.Result;
using Microsoft.Extensions.Options;
using Opilo.HazardZoneMonitor.Api.Features.HazardZones;
using Opilo.HazardZoneMonitor.Api.Features.HazardZones.GetHazardZones;
using Opilo.HazardZoneMonitor.Api.Shared.Configuration;
using Opilo.HazardZoneMonitor.Domain.Shared.Primitives;

namespace Opilo.HazardZoneMonitor.Api.Tests.Unit.Features.HazardZones.GetHazardZones;

public sealed class HandlerTests
{
    [Fact]
    public async Task Handle_ShouldReturnSuccessResult_WhenHazardZonesAreConfigured()
    {
        // Arrange
        PointConfiguration point1 = new(0.0, 0.0);
        PointConfiguration point2 = new(10.0, 10.0);
        PointConfiguration point3 = new(10.0, 0.0);

        HazardZoneConfiguration zone1 = new(
            "Hazard Zone 1",
            new[] { point1, point2, point3 },
            TimeSpan.FromSeconds(30),
            TimeSpan.FromSeconds(10),
            ZoneState.Active,
            AlarmState.Alarm,
            5);

        HazardZoneConfiguration zone2 = new(
            "Hazard Zone 2",
            new[] { point1, point2 },
            TimeSpan.FromSeconds(60),
            TimeSpan.FromSeconds(20),
            ZoneState.Inactive,
            AlarmState.PreAlarm,
            10);

        HazardZoneOptions hazardZoneOptions = new()
        {
            HazardZones = new[] { zone1, zone2 },
        };

        IOptions<HazardZoneOptions> options = Options.Create(hazardZoneOptions);
        Handler handler = new(options);
        Query query = new();

        // Act
        Result<Response> result = await handler.Handle(query, TestContext.Current.CancellationToken);

        // Assert
        result.Status.Should().Be(ResultStatus.Ok);
        result.Value.HazardZones.Should().BeEquivalentTo(new[] { zone1, zone2 });
    }

    [Fact]
    public async Task Handle_ShouldReturnSuccessResultWithEmptyHazardZones_WhenNoHazardZonesAreConfigured()
    {
        // Arrange
        HazardZoneOptions hazardZoneOptions = new()
        {
            HazardZones = Array.Empty<HazardZoneConfiguration>(),
        };

        IOptions<HazardZoneOptions> options = Options.Create(hazardZoneOptions);
        Handler handler = new(options);
        Query query = new();

        // Act
        Result<Response> result = await handler.Handle(query, TestContext.Current.CancellationToken);

        // Assert
        result.Status.Should().Be(ResultStatus.Ok);
        result.Value.HazardZones.Should().BeEmpty();
    }
}
