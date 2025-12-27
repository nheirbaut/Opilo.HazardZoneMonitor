using Opilo.HazardZoneMonitor.Features.FloorManagement.Domain;
using Opilo.HazardZoneMonitor.Features.FloorManagement.Events;
using Opilo.HazardZoneMonitor.Tests.Unit.TestUtilities;
using Opilo.HazardZoneMonitor.Shared.Primitives;

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

    public void Dispose()
    {
        _testFloor?.Dispose();
    }
}
