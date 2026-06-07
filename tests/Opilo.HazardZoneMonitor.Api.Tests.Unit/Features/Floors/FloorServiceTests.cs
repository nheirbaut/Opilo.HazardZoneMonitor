using Microsoft.Extensions.Options;
using NSubstitute;
using Opilo.HazardZoneMonitor.Api.Features.Floors;
using Opilo.HazardZoneMonitor.Api.Features.HazardZones.Services;
using Opilo.HazardZoneMonitor.Domain.Shared.Primitives;
using Opilo.HazardZoneMonitor.Tests.Common.TestUtilities;
using Opilo.HazardZoneMonitor.Tests.Common.TestUtilities.Builders;

namespace Opilo.HazardZoneMonitor.Api.Tests.Unit.Features.Floors;

public sealed class FloorServiceTests
{
    [Fact]
    public void ApplyPersonLocationUpdate_ShouldCallApplyPersonLocationUpdateOnHazardZoneService_WhenPersonIsAddedToFloor()
    {
        // Arrange
        var floorOptions = Options.Create(
            FloorOptionsBuilder.Create()
                .WithFloor("Main Floor", f => f.WithRectangleOutline(0, 0, 10, 10))
                .Build());

        var hazardZoneService = Substitute.For<IHazardZoneService>();
        var clock = new FakeClock();
        var timerFactory = new FakeTimerFactory(clock);

        using var floorService = new FloorService(floorOptions, hazardZoneService, clock, timerFactory);

        var personId = PersonId.From(Guid.NewGuid());
        var coordinate = new Coordinate(5, 5);

        // Act
        floorService.ApplyPersonLocationUpdate(new PersonLocationUpdate(personId, coordinate));

        // Assert
        hazardZoneService.Received(1).ApplyPersonLocationUpdate(Arg.Is<PersonLocationUpdate>(
            update => update.PersonId == personId && update.Coordinate == coordinate));
    }

    [Fact]
    public void ApplyPersonLocationUpdate_ShouldCallRemovePersonOnHazardZoneService_WhenPersonLeavesFloor()
    {
        // Arrange
        var floorOptions = Options.Create(
            FloorOptionsBuilder.Create()
                .WithFloor("Main Floor", f => f.WithRectangleOutline(0, 0, 10, 10))
                .Build());

        var hazardZoneService = Substitute.For<IHazardZoneService>();
        var clock = new FakeClock();
        var timerFactory = new FakeTimerFactory(clock);

        using var floorService = new FloorService(floorOptions, hazardZoneService, clock, timerFactory);

        var personId = PersonId.From(Guid.NewGuid());
        var inside = new Coordinate(5, 5);
        var outside = new Coordinate(15, 15);

        floorService.ApplyPersonLocationUpdate(new PersonLocationUpdate(personId, inside));

        // Act
        floorService.ApplyPersonLocationUpdate(new PersonLocationUpdate(personId, outside));

        // Assert
        hazardZoneService.Received(1).RemovePerson(personId);
    }
}
