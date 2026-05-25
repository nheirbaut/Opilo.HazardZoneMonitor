// ReSharper disable AccessToDisposedClosure

using Opilo.HazardZoneMonitor.Domain.Features.FloorManagement.Domain;
using Opilo.HazardZoneMonitor.Domain.Features.FloorManagement.Events;
using Opilo.HazardZoneMonitor.Domain.Features.HazardZoneManagement.Domain;
using Opilo.HazardZoneMonitor.Domain.Features.HazardZoneManagement.Events;
using Opilo.HazardZoneMonitor.Domain.Shared.Primitives;
using Opilo.HazardZoneMonitor.Tests.Common.TestUtilities;
namespace Opilo.HazardZoneMonitor.Domain.Tests.Unit.Domain;

public sealed class FloorTests : IDisposable
{
    private static readonly Outline s_validOutline = new(
        new([
            new Coordinate(0, 0),
            new Coordinate(4, 0),
            new Coordinate(4, 4),
            new Coordinate(0, 4)
        ]));

    private Floor? _testFloor;
    private readonly FakeClock _clock;
    private readonly FakeTimerFactory _timerFactory;

    private static readonly FloorName s_validFloorName = FloorName.From("TestFloor");

    public FloorTests()
    {
        _clock = new FakeClock();
        _timerFactory = new FakeTimerFactory(_clock);
    }

    [Fact]
    public void Constructor_ShouldThrowArgumentNullException_WhenOutlineIsNull()
    {
        // Act & Assert
        var act = () => new Floor(s_validFloorName, null!, []);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Constructor_ShouldAcceptFloorName_WhenValidNameIsProvided()
    {
        // Arrange
        var floorName = FloorName.From("TestFloor");

        // Act
        _testFloor = new Floor(floorName, s_validOutline, []);

        // Assert
        _testFloor.Name.Should().Be(floorName);
    }

    [Fact]
    public void Constructor_ShouldCreateInstance_WhenEmptyHazardZonesCollectionIsProvided()
    {
        // Act
        _testFloor = new Floor(s_validFloorName, s_validOutline, []);

        // Assert
        _testFloor.Name.Should().Be(s_validFloorName);
    }

    [Fact]
    public void Constructor_ShouldThrowArgumentException_WhenHazardZoneOutlineIsNotWithinFloorOutline()
    {
        // Arrange
        var hazardZoneOutline = new Outline(new([
            new Coordinate(10, 10),
            new Coordinate(12, 10),
            new Coordinate(12, 12),
            new Coordinate(10, 12)
        ]));
        using var hazardZone = new HazardZone(HazardZoneName.From("TestZone"), hazardZoneOutline, Duration.From(TimeSpan.FromSeconds(5)));

        // Act
        var act = () => new Floor(s_validFloorName, s_validOutline, [hazardZone]);

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Constructor_ShouldThrowArgumentException_WhenDuplicateHazardZonesAreProvided()
    {
        // Arrange
        var hazardZoneOutline = new Outline(new([
            new Coordinate(1, 1),
            new Coordinate(3, 1),
            new Coordinate(3, 3),
            new Coordinate(1, 3)
        ]));
        using var hazardZone = new HazardZone(HazardZoneName.From("TestZone"), hazardZoneOutline, Duration.From(TimeSpan.FromSeconds(5)));

        // Act
        var act = () => new Floor(s_validFloorName, s_validOutline, [hazardZone, hazardZone]);

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Constructor_ShouldThrowArgumentException_WhenHazardZonesHaveSameName()
    {
        // Arrange
        var outline1 = new Outline(new([
            new Coordinate(1, 1),
            new Coordinate(2, 1),
            new Coordinate(2, 2),
            new Coordinate(1, 2)
        ]));
        var outline2 = new Outline(new([
            new Coordinate(2.5, 2.5),
            new Coordinate(3.5, 2.5),
            new Coordinate(3.5, 3.5),
            new Coordinate(2.5, 3.5)
        ]));

        using var hazardZone1 = new HazardZone(HazardZoneName.From("SameName"), outline1, Duration.From(TimeSpan.FromSeconds(5)));
        using var hazardZone2 = new HazardZone(HazardZoneName.From("SameName"), outline2, Duration.From(TimeSpan.FromSeconds(5)));

        // Act
        var act = () => new Floor(s_validFloorName, s_validOutline, [hazardZone1, hazardZone2]);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithParameterName("hazardZones")
            .WithMessage("*duplicate*name*");
    }

    [Fact]
    public void Constructor_ShouldThrowArgumentException_WhenHazardZonesOverlap()
    {
        // Arrange
        var floorOutline = new Outline([new(0, 0), new(100, 0), new(100, 100), new(0, 100)]);

        var overlappingOutline1 = new Outline([new(10, 10), new(60, 10), new(60, 60), new(10, 60)]);
        var overlappingOutline2 = new Outline([new(40, 40), new(90, 40), new(90, 90), new(40, 90)]);

        using var hazardZone1 = new HazardZone(HazardZoneName.From("Zone1"), overlappingOutline1, Duration.From(TimeSpan.FromSeconds(5)));
        using var hazardZone2 = new HazardZone(HazardZoneName.From("Zone2"), overlappingOutline2, Duration.From(TimeSpan.FromSeconds(5)));

        // Act
        var act = () => new Floor(FloorName.From("Test Floor"), floorOutline, [hazardZone1, hazardZone2]);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithParameterName("hazardZones")
            .WithMessage("*overlap*");
    }

    [Fact]
    public void TryAddPersonLocationUpdate_ShouldThrowArgumentNullException_WhenPersonLocationUpdateIsNull()
    {
        // Arrange
        _testFloor = new Floor(s_validFloorName, s_validOutline, []);

        // Act & Assert
        var act = () => _testFloor.TryAddPersonLocationUpdate(null!);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void TryAddPersonLocationUpdate_ShouldReturnFalse_WhenPersonLocationUpdateIsNotOnFloor()
    {
        // Arrange
        _testFloor = new Floor(s_validFloorName, s_validOutline, []);
        var personMovement = new PersonLocationUpdate(PersonId.From(Guid.NewGuid()), new Coordinate(8, 8));

        // Act
        var result = _testFloor.TryAddPersonLocationUpdate(personMovement);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void TryAddPersonLocationUpdate_ShouldReturnTrue_WhenPersonLocationUpdateIsOnFloor()
    {
        // Arrange
        _testFloor = new Floor(s_validFloorName, s_validOutline, []);
        var personMovement = new PersonLocationUpdate(PersonId.From(Guid.NewGuid()), new Coordinate(2, 2));

        // Act
        var result = _testFloor.TryAddPersonLocationUpdate(personMovement);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void
        TryAddPersonLocationUpdate_ShouldRaisePersonAddedToFloorEvent_WhenPersonLocationUpdateIsOnFloorAndPersonIsNew()
    {
        // Arrange
        var personId = PersonId.From(Guid.NewGuid());
        var location = new Coordinate(2, 2);
        _testFloor = new Floor(s_validFloorName, s_validOutline, []);
        var personMovement = new PersonLocationUpdate(personId, location);
        PersonAddedToFloorEventArgs? personAddedToFloorEvent = null;
        _testFloor.PersonAddedToFloor += (_, e) => personAddedToFloorEvent = e;

        // Act
        _testFloor.TryAddPersonLocationUpdate(personMovement);

        // Assert
        personAddedToFloorEvent.Should().NotBeNull();
        personAddedToFloorEvent.FloorName.Should().Be(s_validFloorName);
        personAddedToFloorEvent.PersonId.Should().Be(personId);
        personAddedToFloorEvent.Location.Should().Be(location);
    }

    [Fact]
    public void
        TryAddPersonLocationUpdate_ShouldNotRaisePersonAddedToFloorEvent_WhenPersonLocationUpdateIsOnFloorAndPersonIsKnown()
    {
        // Arrange
        var personId = PersonId.From(Guid.NewGuid());
        var location = new Coordinate(2, 2);
        _testFloor = new Floor(s_validFloorName, s_validOutline, []);
        var personMovement = new PersonLocationUpdate(personId, location);
        _testFloor.TryAddPersonLocationUpdate(personMovement);
        PersonAddedToFloorEventArgs? personAddedToFloorEvent = null;
        _testFloor.PersonAddedToFloor += (_, e) => personAddedToFloorEvent = e;

        // Act
        _testFloor.TryAddPersonLocationUpdate(personMovement);

        // Assert
        personAddedToFloorEvent.Should().BeNull();
    }

    [Fact]
    public void TryAddPersonLocationUpdate_ShouldRaisePersonRemovedFromFloorEvent_WhenPersonExpires()
    {
        // Arrange
        var personId = PersonId.From(Guid.NewGuid());
        var location = new Coordinate(2, 2);
        var personTimeout = TimeSpan.FromMilliseconds(10);
        _testFloor = new Floor(s_validFloorName, s_validOutline, [], personTimeout, _timerFactory);
        var personMovement = new PersonLocationUpdate(personId, location);
        PersonRemovedFromFloorEventArgs? personRemovedFromFloorEvent = null;
        _testFloor.PersonRemovedFromFloor += (_, e) => personRemovedFromFloorEvent = e;
        _testFloor.TryAddPersonLocationUpdate(personMovement);

        // Act
        _clock.AdvanceBy(personTimeout * 2);

        // Assert
        personRemovedFromFloorEvent.Should().NotBeNull();
        personRemovedFromFloorEvent.FloorName.Should().Be(s_validFloorName);
        personRemovedFromFloorEvent.PersonId.Should().Be(personId);
    }

    [Fact]
    public void
        TryAddPersonLocationUpdate_ShouldRaisePersonRemovedFromFloorEvent_WhenPersonMovesOffFloorAndPersonIsKnown()
    {
        // Arrange
        var personId = PersonId.From(Guid.NewGuid());
        var locationOnFloor = new Coordinate(2, 2);
        var locationOffFloor = new Coordinate(200, 200);
        _testFloor = new Floor(s_validFloorName, s_validOutline, []);
        var personMovementOnFloor = new PersonLocationUpdate(personId, locationOnFloor);
        var personMovementOffFloor = new PersonLocationUpdate(personId, locationOffFloor);
        _testFloor.TryAddPersonLocationUpdate(personMovementOnFloor);
        PersonRemovedFromFloorEventArgs? personRemovedFromFloorEvent = null;
        _testFloor.PersonRemovedFromFloor += (_, e) => personRemovedFromFloorEvent = e;

        // Act
        _testFloor.TryAddPersonLocationUpdate(personMovementOffFloor);

        // Assert
        personRemovedFromFloorEvent.Should().NotBeNull();
        personRemovedFromFloorEvent.FloorName.Should().Be(s_validFloorName);
        personRemovedFromFloorEvent.PersonId.Should().Be(personId);
    }

    [Fact]
    public void TryAddPersonLocationUpdate_ShouldForwardPersonCreatedEventToHazardZones_WhenNewPersonIsAdded()
    {
        // Arrange
        var floorOutline = new Outline([new(0, 0), new(100, 0), new(100, 100), new(0, 100)]);
        var hazardZoneOutline = new Outline([new(10, 10), new(40, 10), new(40, 40), new(10, 40)]);

        using var hazardZone = new HazardZone(HazardZoneName.From("TestZone"), hazardZoneOutline, Duration.From(TimeSpan.FromSeconds(5)));
        _testFloor = new Floor(FloorName.From("Test Floor"), floorOutline, [hazardZone]);

        var personId = PersonId.From(Guid.NewGuid());
        var location = new Coordinate(20, 20); // Inside hazard zone
        var personLocationUpdate = new PersonLocationUpdate(personId, location);

        var personAddedEvents = new List<PersonAddedToHazardZoneEventArgs>();
        hazardZone.PersonAddedToHazardZone += (_, e) => personAddedEvents.Add(e);

        // Act
        _testFloor.TryAddPersonLocationUpdate(personLocationUpdate);

        // Assert
        var personAddedToHazardZoneEvent = personAddedEvents.Single();
        personAddedToHazardZoneEvent.PersonId.Should().Be(personId);
        personAddedToHazardZoneEvent.HazardZoneName.Should().Be(HazardZoneName.From("TestZone"));
    }

    [Fact]
    public void TryAddPersonLocationUpdate_ShouldForwardPersonLocationChangedEventToHazardZones_WhenExistingPersonMoves()
    {
        // Arrange
        var floorOutline = new Outline([new(0, 0), new(100, 0), new(100, 100), new(0, 100)]);
        var hazardZoneOutline = new Outline([new(10, 10), new(40, 10), new(40, 40), new(10, 40)]);

        using var hazardZone = new HazardZone(HazardZoneName.From("TestZone"), hazardZoneOutline, Duration.From(TimeSpan.FromSeconds(5)));
        _testFloor = new Floor(FloorName.From("Test Floor"), floorOutline, [hazardZone]);

        var personId = PersonId.From(Guid.NewGuid());
        var initialLocation = new Coordinate(50, 50); // Outside hazard zone
        var newLocation = new Coordinate(20, 20); // Inside hazard zone

        // Add person first
        _testFloor.TryAddPersonLocationUpdate(new PersonLocationUpdate(personId, initialLocation));

        PersonAddedToHazardZoneEventArgs? personAddedToHazardZoneEvent = null;
        hazardZone.PersonAddedToHazardZone += (_, e) => personAddedToHazardZoneEvent = e;

        // Act
        _testFloor.TryAddPersonLocationUpdate(new PersonLocationUpdate(personId, newLocation));

        // Assert
        personAddedToHazardZoneEvent.Should().NotBeNull();
        personAddedToHazardZoneEvent.PersonId.Should().Be(personId);
    }

    [Fact]
    public void TryAddPersonLocationUpdate_ShouldForwardPersonExpiredEventToHazardZones_WhenPersonExpires()
    {
        // Arrange
        var floorOutline = new Outline([new(0, 0), new(100, 0), new(100, 100), new(0, 100)]);
        var hazardZoneOutline = new Outline([new(10, 10), new(40, 10), new(40, 40), new(10, 40)]);

        using var hazardZone = new HazardZone(HazardZoneName.From("TestZone"), hazardZoneOutline, Duration.From(TimeSpan.FromSeconds(5)));
        var personTimeout = TimeSpan.FromMilliseconds(10);
        _testFloor = new Floor(FloorName.From("Test Floor"), floorOutline, [hazardZone], personTimeout, _timerFactory);

        var personId = PersonId.From(Guid.NewGuid());
        var location = new Coordinate(20, 20); // Inside hazard zone

        // Add person first
        _testFloor.TryAddPersonLocationUpdate(new PersonLocationUpdate(personId, location));

        PersonRemovedFromHazardZoneEventArgs? personRemovedFromHazardZoneEvent = null;
        hazardZone.PersonRemovedFromHazardZone += (_, e) => personRemovedFromHazardZoneEvent = e;

        // Act
        _clock.AdvanceBy(personTimeout * 2);

        // Assert
        personRemovedFromHazardZoneEvent.Should().NotBeNull();
        personRemovedFromHazardZoneEvent.PersonId.Should().Be(personId);
    }

    public void Dispose()
    {
        _testFloor?.Dispose();
    }
}
