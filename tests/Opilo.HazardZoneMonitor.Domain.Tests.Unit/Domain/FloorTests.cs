using Opilo.HazardZoneMonitor.Domain.Features.FloorManagement.Domain;
using Opilo.HazardZoneMonitor.Domain.Features.FloorManagement.Events;
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
        var act = () => new Floor(s_validFloorName, null!);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Constructor_ShouldAcceptFloorName_WhenValidNameIsProvided()
    {
        // Arrange
        var floorName = FloorName.From("TestFloor");

        // Act
        _testFloor = new Floor(floorName, s_validOutline);

        // Assert
        _testFloor.Name.Should().Be(floorName);
    }

    [Fact]
    public void TryAddPersonLocationUpdate_ShouldThrowArgumentNullException_WhenPersonLocationUpdateIsNull()
    {
        // Arrange
        _testFloor = new Floor(s_validFloorName, s_validOutline);

        // Act & Assert
        var act = () => _testFloor.TryAddPersonLocationUpdate(null!);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void TryAddPersonLocationUpdate_ShouldReturnFalse_WhenPersonLocationUpdateIsNotOnFloor()
    {
        // Arrange
        _testFloor = new Floor(s_validFloorName, s_validOutline);
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
        _testFloor = new Floor(s_validFloorName, s_validOutline);
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
        _testFloor = new Floor(s_validFloorName, s_validOutline);
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
        _testFloor = new Floor(s_validFloorName, s_validOutline);
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
    public void TryAddPersonLocationUpdate_ShouldRaisePersonLocationChangedEvent_WhenExistingPersonMoves()
    {
        // Arrange
        var personId = PersonId.From(Guid.NewGuid());
        var initialLocation = new Coordinate(2, 2);
        var newLocation = new Coordinate(3, 3);
        _testFloor = new Floor(s_validFloorName, s_validOutline);
        _testFloor.TryAddPersonLocationUpdate(new PersonLocationUpdate(personId, initialLocation));

        PersonLocationChangedOnFloorEventArgs? personLocationChangedEvent = null;
        _testFloor.PersonLocationChanged += (_, e) => personLocationChangedEvent = e;

        // Act
        _testFloor.TryAddPersonLocationUpdate(new PersonLocationUpdate(personId, newLocation));

        // Assert
        personLocationChangedEvent.Should().NotBeNull();
        personLocationChangedEvent.FloorName.Should().Be(s_validFloorName);
        personLocationChangedEvent.PersonId.Should().Be(personId);
        personLocationChangedEvent.Location.Should().Be(newLocation);
    }

    [Fact]
    public void TryAddPersonLocationUpdate_ShouldRaisePersonRemovedFromFloorEvent_WhenPersonExpires()
    {
        // Arrange
        var personId = PersonId.From(Guid.NewGuid());
        var location = new Coordinate(2, 2);
        var personTimeout = TimeSpan.FromMilliseconds(10);
        _testFloor = new Floor(s_validFloorName, s_validOutline, personTimeout, _timerFactory);
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
        _testFloor = new Floor(s_validFloorName, s_validOutline);
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

    public void Dispose()
    {
        _testFloor?.Dispose();
    }
}
