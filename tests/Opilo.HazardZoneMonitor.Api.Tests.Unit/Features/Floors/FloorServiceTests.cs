using Microsoft.Extensions.Options;
using NSubstitute;
using Opilo.HazardZoneMonitor.Api.Features.Floors;
using Opilo.HazardZoneMonitor.Api.Features.Floors.Configuration;
using Opilo.HazardZoneMonitor.Api.Features.HazardZones.Services;
using Opilo.HazardZoneMonitor.Domain.Shared.Primitives;
using Opilo.HazardZoneMonitor.Tests.Common.TestUtilities;

namespace Opilo.HazardZoneMonitor.Api.Tests.Unit.Features.Floors;

public sealed class FloorServiceTests
{
    [Fact]
    public void ApplyPersonLocationUpdate_ShouldCallApplyPersonLocationUpdateOnHazardZoneService_WhenPersonIsAddedToFloor()
    {
        // Arrange
        var floorName = FloorName.From("Main Floor");
        var floorOptions = Options.Create(new FloorOptions
        {
            Floors =
            [
                new FloorConfiguration(
                    floorName,
                    [new Coordinate(0, 0), new Coordinate(10, 0), new Coordinate(10, 10), new Coordinate(0, 10)])
            ]
        });

        var hazardZoneService = Substitute.For<IHazardZoneService>();
        var timerFactory = new FakeTimerFactory(new FakeClock());

        using var floorService = new FloorService(floorOptions, hazardZoneService, timerFactory);

        var personId = PersonId.From(Guid.NewGuid());
        var coordinate = new Coordinate(5, 5);

        // Act
        floorService.ApplyPersonLocationUpdate(new PersonLocationUpdate(personId, coordinate));

        // Assert
        hazardZoneService.Received(1).ApplyPersonLocationUpdate(Arg.Is<PersonLocationUpdate>(
            update => update.PersonId == personId && update.Coordinate == coordinate));
    }
}
