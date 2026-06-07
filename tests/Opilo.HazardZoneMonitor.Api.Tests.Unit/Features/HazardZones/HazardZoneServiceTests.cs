using Ardalis.Result;
using Microsoft.Extensions.Options;
using Opilo.HazardZoneMonitor.Api.Features.HazardZones;
using Opilo.HazardZoneMonitor.Api.Features.HazardZones.Configuration;
using Opilo.HazardZoneMonitor.Domain.Shared.Primitives;
using Opilo.HazardZoneMonitor.Domain.Shared.Time;
using Opilo.HazardZoneMonitor.Tests.Common.TestUtilities;

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
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void ApplyPersonLocationUpdate_ShouldUpdateHazardZoneAlarmState_WhenPersonIsInsideActiveZoneAndOverThreshold()
    {
        // Arrange
        var hazardZoneName = HazardZoneName.From("TestZone");
        var clock = new FakeClock();
        var options = Options.Create(new HazardZoneOptions
        {
            HazardZones =
            [
                new HazardZoneConfiguration(
                    hazardZoneName,
                    [new Coordinate(0, 0), new Coordinate(10, 0), new Coordinate(10, 10), new Coordinate(0, 10)],
                    TimeSpan.Zero,
                    TimeSpan.Zero,
                    AllowedNumberOfPersons: 0)
            ]
        });

        using var hazardZoneService = new HazardZoneService(options, clock, new FakeTimerFactory(clock));
        hazardZoneService.ActivateHazardZone(hazardZoneName);

        var personId = PersonId.From(Guid.NewGuid());
        var location = new Coordinate(5, 5);

        // Act
        hazardZoneService.ApplyPersonLocationUpdate(new PersonLocationUpdate(personId, location));

        // Assert
        var hazardZone = hazardZoneService.GetHazardZones().Single();
        hazardZone.AlarmState.Should().Be(AlarmState.Alarm);
    }

    [Fact]
    public void ApplyPersonLocationUpdate_ShouldTransitionAlarmStateToNone_WhenPersonMovesOutsideActiveZone()
    {
        // Arrange
        var hazardZoneName = HazardZoneName.From("TestZone");
        var clock = new FakeClock();
        var options = Options.Create(new HazardZoneOptions
        {
            HazardZones =
            [
                new HazardZoneConfiguration(
                    hazardZoneName,
                    [new Coordinate(0, 0), new Coordinate(10, 0), new Coordinate(10, 10), new Coordinate(0, 10)],
                    TimeSpan.Zero,
                    TimeSpan.Zero,
                    AllowedNumberOfPersons: 0)
            ]
        });

        using var hazardZoneService = new HazardZoneService(options, clock, new FakeTimerFactory(clock));
        hazardZoneService.ActivateHazardZone(hazardZoneName);

        var personId = PersonId.From(Guid.NewGuid());
        var insideLocation = new Coordinate(5, 5);
        var outsideLocation = new Coordinate(15, 15);

        hazardZoneService.ApplyPersonLocationUpdate(new PersonLocationUpdate(personId, insideLocation));
        var hazardZone = hazardZoneService.GetHazardZones().Single();
        hazardZone.AlarmState.Should().Be(AlarmState.Alarm);

        // Act
        hazardZoneService.ApplyPersonLocationUpdate(new PersonLocationUpdate(personId, outsideLocation));

        // Assert
        hazardZone = hazardZoneService.GetHazardZones().Single();
        hazardZone.AlarmState.Should().Be(AlarmState.None);
    }
}
