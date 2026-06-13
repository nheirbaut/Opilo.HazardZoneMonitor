using Ardalis.Result;
using Microsoft.Extensions.Options;
using Opilo.HazardZoneMonitor.Api.Features.HazardZones;
using Opilo.HazardZoneMonitor.Domain.Shared.Primitives;
using Opilo.HazardZoneMonitor.Domain.Shared.Time;
using Opilo.HazardZoneMonitor.Tests.Common.TestUtilities;
using Opilo.HazardZoneMonitor.Tests.Common.TestUtilities.Builders;

namespace Opilo.HazardZoneMonitor.Api.Tests.Unit.Features.HazardZones;

public sealed class HazardZoneServiceTests
{
    [Fact]
    public void ActivateHazardZone_ShouldReturnNotFoundResult_WhenNoHazardZonesAreConfigured()
    {
        // Arrange
        var hazardZoneOptions = Options.Create(HazardZoneOptionsBuilder.Create().Build());
        var floorOptions = Options.Create(FloorOptionsBuilder.Create().Build());
        using var hazardZoneService = new HazardZoneService(hazardZoneOptions, floorOptions, new SystemClock(), new SystemTimerFactory());
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
        var hazardZoneOptions = Options.Create(
            HazardZoneOptionsBuilder.Create()
                .WithHazardZone("existing-hazardzone", z => z.WithRectangleOutline(0, 0, 10, 10))
                .Build());
        var floorOptions = Options.Create(FloorOptionsBuilder.Create().Build());

        using var hazardZoneService = new HazardZoneService(hazardZoneOptions, floorOptions, new SystemClock(), new SystemTimerFactory());

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
        var hazardZoneOptions = Options.Create(
            HazardZoneOptionsBuilder.Create()
                .WithHazardZone("TestZone", z => z
                    .WithRectangleOutline(0, 0, 10, 10)
                    .WithAllowedNumberOfPersons(0))
                .Build());
        var floorOptions = Options.Create(FloorOptionsBuilder.Create().Build());

        using var hazardZoneService = new HazardZoneService(hazardZoneOptions, floorOptions, clock, new FakeTimerFactory(clock));
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
        var hazardZoneOptions = Options.Create(
            HazardZoneOptionsBuilder.Create()
                .WithHazardZone("TestZone", z => z
                    .WithRectangleOutline(0, 0, 10, 10)
                    .WithAllowedNumberOfPersons(0))
                .Build());
        var floorOptions = Options.Create(FloorOptionsBuilder.Create().Build());

        using var hazardZoneService = new HazardZoneService(hazardZoneOptions, floorOptions, clock, new FakeTimerFactory(clock));
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

    [Fact]
    public void RemovePerson_ShouldTransitionAlarmStateToNone_WhenPersonIsInsideActiveZone()
    {
        // Arrange
        var hazardZoneName = HazardZoneName.From("TestZone");
        var clock = new FakeClock();
        var hazardZoneOptions = Options.Create(
            HazardZoneOptionsBuilder.Create()
                .WithHazardZone("TestZone", z => z
                    .WithRectangleOutline(0, 0, 10, 10)
                    .WithAllowedNumberOfPersons(0))
                .Build());
        var floorOptions = Options.Create(FloorOptionsBuilder.Create().Build());

        using var hazardZoneService = new HazardZoneService(hazardZoneOptions, floorOptions, clock, new FakeTimerFactory(clock));
        hazardZoneService.ActivateHazardZone(hazardZoneName);

        var personId = PersonId.From(Guid.NewGuid());
        var location = new Coordinate(5, 5);

        hazardZoneService.ApplyPersonLocationUpdate(new PersonLocationUpdate(personId, location));
        var hazardZone = hazardZoneService.GetHazardZones().Single();
        hazardZone.AlarmState.Should().Be(AlarmState.Alarm);

        // Act
        hazardZoneService.RemovePerson(personId);

        // Assert
        hazardZone = hazardZoneService.GetHazardZones().Single();
        hazardZone.AlarmState.Should().Be(AlarmState.None);
    }

    [Fact]
    public void Constructor_ShouldCreateHazardZonesFromFloorOptions_WhenFloorHasHazardZones()
    {
        // Arrange
        var clock = new FakeClock();
        var hazardZoneOptions = Options.Create(HazardZoneOptionsBuilder.Create().Build());
        var floorOptions = Options.Create(
            FloorOptionsBuilder.Create()
                .WithFloor("Main Floor", f => f
                    .WithRectangleOutline(0, 0, 100, 100)
                    .WithHazardZone("FloorZone", z => z
                        .WithRectangleOutline(10, 10, 20, 20)
                        .WithAllowedNumberOfPersons(0)))
                .Build());

        // Act
        using var hazardZoneService = new HazardZoneService(hazardZoneOptions, floorOptions, clock, new FakeTimerFactory(clock));

        // Assert
        var hazardZones = hazardZoneService.GetHazardZones();
        hazardZones.Should().ContainSingle();
        hazardZones[0].Name.Should().Be(HazardZoneName.From("FloorZone"));
    }

    [Fact]
    public void Constructor_ShouldThrowInvalidOperationException_WhenDuplicateHazardZoneNamesAcrossSources()
    {
        // Arrange
        var clock = new FakeClock();
        var hazardZoneOptions = Options.Create(
            HazardZoneOptionsBuilder.Create()
                .WithHazardZone("DuplicateZone", z => z.WithRectangleOutline(0, 0, 10, 10))
                .Build());
        var floorOptions = Options.Create(
            FloorOptionsBuilder.Create()
                .WithFloor("Main Floor", f => f
                    .WithRectangleOutline(0, 0, 100, 100)
                    .WithHazardZone("DuplicateZone", z => z.WithRectangleOutline(20, 20, 30, 30)))
                .Build());

        // Act
        var act = () => new HazardZoneService(hazardZoneOptions, floorOptions, clock, new FakeTimerFactory(clock));

        // Assert
        act.Should().Throw<InvalidOperationException>();
    }
}
