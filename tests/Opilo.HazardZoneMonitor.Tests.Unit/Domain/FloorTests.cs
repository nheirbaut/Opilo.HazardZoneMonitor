using Opilo.HazardZoneMonitor.Features.FloorManagement.Domain;
using Opilo.HazardZoneMonitor.Features.FloorManagement.Events;
using Opilo.HazardZoneMonitor.Features.HazardZoneManagement.Domain;
using Opilo.HazardZoneMonitor.Features.HazardZoneManagement.Events;
using Opilo.HazardZoneMonitor.Tests.Unit.TestUtilities;
using Opilo.HazardZoneMonitor.Shared.Primitives;
// ReSharper disable AccessToDisposedClosure

namespace Opilo.HazardZoneMonitor.Tests.Unit.Domain;

public sealed class FloorTests : IDisposable
{
    private static readonly Outline s_validOutline = new(
        new([
            new Location(0, 0),
            new Location(4, 0),
            new Location(4, 4),
            new Location(0, 4)
        ]));

    private Floor? _testFloor;
    private readonly FakeClock _clock;
    private readonly FakeTimerFactory _timerFactory;

    private const string ValidFloorName = "TestFloor";

    public FloorTests()
    {
        _clock = new FakeClock();
        _timerFactory = new FakeTimerFactory(_clock);
    }

    [Fact]
    public void Constructor_ShouldThrowArgumentNullException_WhenNameIsNull()
    {
        // Act & Assert
        var act = () => new Floor(null!, s_validOutline, []);
        act.Should().Throw<ArgumentNullException>();
    }

    [Theory]
    [ClassData(typeof(InvalidNames))]
    public void Constructor_ShouldThrowArgumentException_WhenNameIsInvalid(string invalidName)
    {
        // Act & Assert
        var act = () => new Floor(invalidName, s_validOutline, []);
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Constructor_ShouldThrowArgumentNullException_WhenOutlineIsNull()
    {
        // Act & Assert
        var act = () => new Floor(ValidFloorName, null!, []);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Constructor_ShouldCreateInstance_WhenValidNameAndOutlineAreProvided()
    {
        // Act
        _testFloor = new Floor(ValidFloorName, s_validOutline, []);

        // Assert
        _testFloor.Name.Should().Be(ValidFloorName);
        _testFloor.Outline.Should().Be(s_validOutline);
    }

    [Fact]
    public void Constructor_ShouldCreateInstance_WhenEmptyHazardZonesCollectionIsProvided()
    {
        // Act
        _testFloor = new Floor(ValidFloorName, s_validOutline, []);

        // Assert
        _testFloor.Name.Should().Be(ValidFloorName);
    }

    [Fact]
    public void Constructor_ShouldThrowArgumentException_WhenHazardZoneOutlineIsNotWithinFloorOutline()
    {
        // Arrange
        var hazardZoneOutline = new Outline(new([
            new Location(10, 10),
            new Location(12, 10),
            new Location(12, 12),
            new Location(10, 12)
        ]));
        using var hazardZone = new HazardZone("TestZone", hazardZoneOutline, TimeSpan.FromSeconds(5));

        // Act
        var act = () => new Floor(ValidFloorName, s_validOutline, [hazardZone]);

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Constructor_ShouldThrowArgumentException_WhenDuplicateHazardZonesAreProvided()
    {
        // Arrange
        var hazardZoneOutline = new Outline(new([
            new Location(1, 1),
            new Location(3, 1),
            new Location(3, 3),
            new Location(1, 3)
        ]));
        using var hazardZone = new HazardZone("TestZone", hazardZoneOutline, TimeSpan.FromSeconds(5));

        // Act
        var act = () => new Floor(ValidFloorName, s_validOutline, [hazardZone, hazardZone]);

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Constructor_ShouldThrowArgumentException_WhenHazardZonesOverlap()
    {
        // Arrange
        var floorOutline = new Outline([new(0, 0), new(100, 0), new(100, 100), new(0, 100)]);

        var overlappingOutline1 = new Outline([new(10, 10), new(60, 10), new(60, 60), new(10, 60)]);
        var overlappingOutline2 = new Outline([new(40, 40), new(90, 40), new(90, 90), new(40, 90)]);

        using var hazardZone1 = new HazardZone("Zone1", overlappingOutline1, TimeSpan.FromSeconds(5));
        using var hazardZone2 = new HazardZone("Zone2", overlappingOutline2, TimeSpan.FromSeconds(5));

        // Act
        var act = () => new Floor("Test Floor", floorOutline, [hazardZone1, hazardZone2]);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithParameterName("hazardZones")
            .WithMessage("*overlap*");
    }

    [Fact]
    public void TryAddPersonLocationUpdate_ShouldThrowArgumentNullException_WhenPersonLocationUpdateIsNull()
    {
        // Arrange
        _testFloor = new Floor(ValidFloorName, s_validOutline, []);

        // Act & Assert
        var act = () => _testFloor.TryAddPersonLocationUpdate(null!);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void TryAddPersonLocationUpdate_ShouldReturnFalse_WhenPersonLocationUpdateIsNotOnFloor()
    {
        // Arrange
        _testFloor = new Floor(ValidFloorName, s_validOutline, []);
        var personMovement = new PersonLocationUpdate(Guid.NewGuid(), new Location(8, 8));

        // Act
        var result = _testFloor.TryAddPersonLocationUpdate(personMovement);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void TryAddPersonLocationUpdate_ShouldReturnTrue_WhenPersonLocationUpdateIsOnFloor()
    {
        // Arrange
        _testFloor = new Floor(ValidFloorName, s_validOutline, []);
        var personMovement = new PersonLocationUpdate(Guid.NewGuid(), new Location(2, 2));

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
        var personId = Guid.NewGuid();
        var location = new Location(2, 2);
        _testFloor = new Floor(ValidFloorName, s_validOutline, []);
        var personMovement = new PersonLocationUpdate(personId, location);
        PersonAddedToFloorEventArgs? personAddedToFloorEvent = null;
        _testFloor.PersonAddedToFloor += (_, e) => personAddedToFloorEvent = e;

        // Act
        _testFloor.TryAddPersonLocationUpdate(personMovement);

        // Assert
        personAddedToFloorEvent.Should().NotBeNull();
        personAddedToFloorEvent.FloorName.Should().Be(ValidFloorName);
        personAddedToFloorEvent.PersonId.Should().Be(personId);
        personAddedToFloorEvent.Location.Should().Be(location);
    }

    [Fact]
    public void
        TryAddPersonLocationUpdate_ShouldNotRaisePersonAddedToFloorEvent_WhenPersonLocationUpdateIsOnFloorAndPersonIsKnown()
    {
        // Arrange
        var personId = Guid.NewGuid();
        var location = new Location(2, 2);
        _testFloor = new Floor(ValidFloorName, s_validOutline, []);
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
        var personId = Guid.NewGuid();
        var location = new Location(2, 2);
        var personTimeout = TimeSpan.FromMilliseconds(10);
        _testFloor = new Floor(ValidFloorName, s_validOutline, [], personTimeout, _clock, _timerFactory);
        var personMovement = new PersonLocationUpdate(personId, location);
        PersonRemovedFromFloorEventArgs? personRemovedFromFloorEvent = null;
        _testFloor.PersonRemovedFromFloor += (_, e) => personRemovedFromFloorEvent = e;
        _testFloor.TryAddPersonLocationUpdate(personMovement);

        // Act
        _clock.AdvanceBy(personTimeout * 2);

        // Assert
        personRemovedFromFloorEvent.Should().NotBeNull();
        personRemovedFromFloorEvent.FloorName.Should().Be(ValidFloorName);
        personRemovedFromFloorEvent.PersonId.Should().Be(personId);
    }

    [Fact]
    public void
        TryAddPersonLocationUpdate_ShouldRaisePersonRemovedFromFloorEvent_WhenPersonMovesOffFloorAndPersonIsKnown()
    {
        // Arrange
        var personId = Guid.NewGuid();
        var locationOnFloor = new Location(2, 2);
        var locationOffFloor = new Location(200, 200);
        _testFloor = new Floor(ValidFloorName, s_validOutline, []);
        var personMovementOnFloor = new PersonLocationUpdate(personId, locationOnFloor);
        var personMovementOffFloor = new PersonLocationUpdate(personId, locationOffFloor);
        _testFloor.TryAddPersonLocationUpdate(personMovementOnFloor);
        PersonRemovedFromFloorEventArgs? personRemovedFromFloorEvent = null;
        _testFloor.PersonRemovedFromFloor += (_, e) => personRemovedFromFloorEvent = e;

        // Act
        _testFloor.TryAddPersonLocationUpdate(personMovementOffFloor);

        // Assert
        personRemovedFromFloorEvent.Should().NotBeNull();
        personRemovedFromFloorEvent.FloorName.Should().Be(ValidFloorName);
        personRemovedFromFloorEvent.PersonId.Should().Be(personId);
    }

    [Fact]
    public async Task TryAddPersonLocationUpdate_ShouldForwardPersonCreatedEventToHazardZones_WhenNewPersonIsAdded()
    {
        // Arrange
        var floorOutline = new Outline([new(0, 0), new(100, 0), new(100, 100), new(0, 100)]);
        var hazardZoneOutline = new Outline([new(10, 10), new(40, 10), new(40, 40), new(10, 40)]);

        using var hazardZone = new HazardZone("TestZone", hazardZoneOutline, TimeSpan.FromSeconds(5));
        _testFloor = new Floor("Test Floor", floorOutline, [hazardZone]);

        var personId = Guid.NewGuid();
        var location = new Location(20, 20); // Inside hazard zone
        var personLocationUpdate = new PersonLocationUpdate(personId, location);

        var personAddedToHazardZoneEventTask = EventsExtensions.RegisterAndWaitForEvent<PersonAddedToHazardZoneEventArgs>(
            h => hazardZone.PersonAddedToHazardZone += h,
            h => hazardZone.PersonAddedToHazardZone -= h);

        // Act
        _testFloor.TryAddPersonLocationUpdate(personLocationUpdate);
        var personAddedToHazardZoneEvent = await personAddedToHazardZoneEventTask;

        // Assert
        personAddedToHazardZoneEvent.Should().NotBeNull();
        personAddedToHazardZoneEvent.PersonId.Should().Be(personId);
        personAddedToHazardZoneEvent.HazardZoneName.Should().Be("TestZone");
    }

    [Fact]
    public void TryAddPersonLocationUpdate_ShouldForwardPersonLocationChangedEventToHazardZones_WhenExistingPersonMoves()
    {
        // Arrange
        var floorOutline = new Outline([new(0, 0), new(100, 0), new(100, 100), new(0, 100)]);
        var hazardZoneOutline = new Outline([new(10, 10), new(40, 10), new(40, 40), new(10, 40)]);

        using var hazardZone = new HazardZone("TestZone", hazardZoneOutline, TimeSpan.FromSeconds(5));
        _testFloor = new Floor("Test Floor", floorOutline, [hazardZone]);

        var personId = Guid.NewGuid();
        var initialLocation = new Location(50, 50); // Outside hazard zone
        var newLocation = new Location(20, 20); // Inside hazard zone

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

        using var hazardZone = new HazardZone("TestZone", hazardZoneOutline, TimeSpan.FromSeconds(5));
        var personTimeout = TimeSpan.FromMilliseconds(10);
        _testFloor = new Floor("Test Floor", floorOutline, [hazardZone], personTimeout, _clock, _timerFactory);

        var personId = Guid.NewGuid();
        var location = new Location(20, 20); // Inside hazard zone

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

    [Fact]
    public void Floor_ShouldExposePersonAddedToHazardZoneEvent_WhenHazardZoneRaisesEvent()
    {
        // Arrange
        var floorOutline = new Outline([new(0, 0), new(100, 0), new(100, 100), new(0, 100)]);
        var hazardZoneOutline = new Outline([new(10, 10), new(40, 10), new(40, 40), new(10, 40)]);

        using var hazardZone = new HazardZone("TestZone", hazardZoneOutline, TimeSpan.FromSeconds(5));
        _testFloor = new Floor("Test Floor", floorOutline, [hazardZone]);

        var personId = Guid.NewGuid();
        var location = new Location(20, 20); // Inside hazard zone

        PersonAddedToHazardZoneEventArgs? eventArgs = null;
        _testFloor.PersonAddedToHazardZone += (_, e) => eventArgs = e;

        // Act
        _testFloor.TryAddPersonLocationUpdate(new PersonLocationUpdate(personId, location));

        // Assert
        eventArgs.Should().NotBeNull();
        eventArgs.PersonId.Should().Be(personId);
        eventArgs.HazardZoneName.Should().Be("TestZone");
    }

    public void Dispose()
    {
        _testFloor?.Dispose();
    }
}
