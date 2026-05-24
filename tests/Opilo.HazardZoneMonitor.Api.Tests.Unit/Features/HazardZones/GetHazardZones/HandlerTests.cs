using Ardalis.Result;
using NSubstitute;
using Opilo.HazardZoneMonitor.Api.Features.HazardZones.Configuration;
using Opilo.HazardZoneMonitor.Api.Features.HazardZones.GetHazardZones;
using Opilo.HazardZoneMonitor.Api.Features.HazardZones.Services;
using Opilo.HazardZoneMonitor.Domain.Shared.Primitives;

namespace Opilo.HazardZoneMonitor.Api.Tests.Unit.Features.HazardZones.GetHazardZones;

public sealed class HandlerTests
{
    [Fact]
    public async Task Handle_ShouldReturnSuccessResult_WhenHazardZonesAreConfigured()
    {
        // Arrange
        Coordinate point1 = new(0.0, 0.0);
        Coordinate point2 = new(10.0, 10.0);
        Coordinate point3 = new(10.0, 0.0);

        HazardZoneConfiguration zone1 = new(
            HazardZoneName.From("Hazard Zone 1"),
            new[] { point1, point2, point3 },
            TimeSpan.FromSeconds(30),
            TimeSpan.FromSeconds(10),

            5);

        HazardZoneConfiguration zone2 = new(
            HazardZoneName.From("Hazard Zone 2"),
            new[] { point1, point2 },
            TimeSpan.FromSeconds(60),
            TimeSpan.FromSeconds(20),
            10);

        var hazardZoneService = Substitute.For<IHazardZoneService>();
        hazardZoneService.GetHazardZones().Returns([zone1, zone2]);

        Handler handler = new(hazardZoneService);
        Query query = new();

        // Act
        var result = await handler.Handle(query, TestContext.Current.CancellationToken);

        // Assert
        result.Status.Should().Be(ResultStatus.Ok);
        result.Value.HazardZones.Should().BeEquivalentTo(new[] { zone1, zone2 });
    }

    [Fact]
    public async Task Handle_ShouldReturnSuccessResultWithEmptyHazardZones_WhenNoHazardZonesAreConfigured()
    {
        // Arrange
        var hazardZoneService = Substitute.For<IHazardZoneService>();
        hazardZoneService.GetHazardZones().Returns(Array.Empty<HazardZoneConfiguration>());

        Handler handler = new(hazardZoneService);
        Query query = new();

        // Act
        var result = await handler.Handle(query, TestContext.Current.CancellationToken);

        // Assert
        result.Status.Should().Be(ResultStatus.Ok);
        result.Value.HazardZones.Should().BeEmpty();
    }
}
