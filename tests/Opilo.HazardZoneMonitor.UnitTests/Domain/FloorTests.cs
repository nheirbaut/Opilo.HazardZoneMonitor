using Opilo.HazardZoneMonitor.Features.FloorManagement.Domain;
using Opilo.HazardZoneMonitor.Features.PersonTracking.Events;
using Opilo.HazardZoneMonitor.Features.FloorManagement.Events;
using Opilo.HazardZoneMonitor.UnitTests.TestUtilities;
using Opilo.HazardZoneMonitor.Shared.Primitives;

namespace Opilo.HazardZoneMonitor.UnitTests.Domain;

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

    private const string ValidFloorName = "TestFloor";

    [Fact]
    public void Constructor_ShouldThrowArgumentNullException_WhenNameIsNull()
    {
        // Act & Assert
        var act = () => new Floor(null!, s_validOutline, new PersonEvents());
        act.Should().Throw<ArgumentNullException>();
    }

    [Theory]
    [ClassData(typeof(InvalidNames))]
    public void Constructor_ShouldThrowArgumentException_WhenNameIsInvalid(string invalidName)
    {
        // Act & Assert
        var act = () => new Floor(invalidName, s_validOutline, new PersonEvents());
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Constructor_ShouldThrowArgumentNullException_WhenOutlineIsNull()
    {
        // Act & Assert
        var act = () => new Floor(ValidFloorName, null!, new PersonEvents());
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Constructor_ShouldCreateInstance_WhenValidNameAndOutlineAreProvided()
    {
        // Act
        _testFloor = new Floor(ValidFloorName, s_validOutline, new PersonEvents());

        // Assert
        _testFloor.Name.Should().Be(ValidFloorName);
        _testFloor.Outline.Should().Be(s_validOutline);
    }

    [Fact]
    public void TryAddPersonLocationUpdate_ShouldThrowArgumentNullException_WhenPersonLocationUpdateIsNull()
    {
        // Arrange
        _testFloor = new Floor(ValidFloorName, s_validOutline, new PersonEvents());

        // Act & Assert
        var act = () => _testFloor.TryAddPersonLocationUpdate(null!);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void TryAddPersonLocationUpdate_ShouldReturnFalse_WhenPersonLocationUpdateIsNotOnFloor()
    {
        // Arrange
        _testFloor = new Floor(ValidFloorName, s_validOutline, new PersonEvents());
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
        _testFloor = new Floor(ValidFloorName, s_validOutline, new PersonEvents());
        var personMovement = new PersonLocationUpdate(Guid.NewGuid(), new Location(2, 2));

        // Act
        var result = _testFloor.TryAddPersonLocationUpdate(personMovement);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task
        TryAddPersonLocationUpdate_ShouldRaisePersonAddedToFloorEvent_WhenPersonLocationUpdateIsOnFloorAndPersonIsNew()
    {
        // Arrange
        var personId = Guid.NewGuid();
        var location = new Location(2, 2);
        _testFloor = new Floor(ValidFloorName, s_validOutline, new PersonEvents());
        var personMovement = new PersonLocationUpdate(personId, location);
        var personAddedToFloorEventTask = DomainEventsExtensions.RegisterAndWaitForEvent<PersonAddedToFloorEvent>(
            h => _testFloor.PersonAddedToFloor += h,
            h => _testFloor.PersonAddedToFloor -= h);

        // Act
        _testFloor.TryAddPersonLocationUpdate(personMovement);
        var personAddedToFloorEvent = await personAddedToFloorEventTask;

        // Assert
        personAddedToFloorEvent.Should().NotBeNull();
        personAddedToFloorEvent.FloorName.Should().Be(ValidFloorName);
        personAddedToFloorEvent.PersonId.Should().Be(personId);
        personAddedToFloorEvent.Location.Should().Be(location);
    }

    [Fact]
    public async Task
        TryAddPersonLocationUpdate_ShouldNotRaisePersonAddedToFloorEvent_WhenPersonLocationUpdateIsOnFloorAndPersonIsKnown()
    {
        // Arrange
        var personId = Guid.NewGuid();
        var location = new Location(2, 2);
        _testFloor = new Floor(ValidFloorName, s_validOutline, new PersonEvents());
        var personMovement = new PersonLocationUpdate(personId, location);
        _testFloor.TryAddPersonLocationUpdate(personMovement);

        var personAddedToFloorEventTask = DomainEventsExtensions.RegisterAndWaitForEvent<PersonAddedToFloorEvent>(
            h => _testFloor.PersonAddedToFloor += h,
            h => _testFloor.PersonAddedToFloor -= h,
            TimeSpan.FromMilliseconds(50));

        // Act
        _testFloor.TryAddPersonLocationUpdate(personMovement);
        var personAddedToFloorEvent = await personAddedToFloorEventTask;

        // Assert
        personAddedToFloorEvent.Should().BeNull();
    }

    [Fact]
    public async Task TryAddPersonLocationUpdate_ShouldRaisePersonRemovedFromFloorEvent_WhenPersonExpires()
    {
        // Arrange
        var personId = Guid.NewGuid();
        var location = new Location(2, 2);
        _testFloor = new Floor(ValidFloorName, s_validOutline, new PersonEvents(), TimeSpan.FromMilliseconds(10));
        var personMovement = new PersonLocationUpdate(personId, location);

        var personRemovedFromFloorEventTask = DomainEventsExtensions.RegisterAndWaitForEvent<PersonRemovedFromFloorEvent>(
            h => _testFloor.PersonRemovedFromFloor += h,
            h => _testFloor.PersonRemovedFromFloor -= h);

        // Act
        _testFloor.TryAddPersonLocationUpdate(personMovement);
        var personRemovedFromFloorEvent = await personRemovedFromFloorEventTask;

        // Assert
        personRemovedFromFloorEvent.Should().NotBeNull();
        personRemovedFromFloorEvent.FloorName.Should().Be(ValidFloorName);
        personRemovedFromFloorEvent.PersonId.Should().Be(personId);
    }

    [Fact]
    public async Task
        TryAddPersonLocationUpdate_ShouldRaisePersonRemovedFromFloorEvent_WhenPersonMovesOffFloorAndPersonIsKnown()
    {
        // Arrange
        var personId = Guid.NewGuid();
        var locationOnFloor = new Location(2, 2);
        var locationOffFloor = new Location(200, 200);
        _testFloor = new Floor(ValidFloorName, s_validOutline, new PersonEvents());
        var personMovementOnFloor = new PersonLocationUpdate(personId, locationOnFloor);
        var personMovementOffFloor = new PersonLocationUpdate(personId, locationOffFloor);
        _testFloor.TryAddPersonLocationUpdate(personMovementOnFloor);

        var personRemovedFromFloorEventTask = DomainEventsExtensions.RegisterAndWaitForEvent<PersonRemovedFromFloorEvent>(
            h => _testFloor.PersonRemovedFromFloor += h,
            h => _testFloor.PersonRemovedFromFloor -= h,
            TimeSpan.FromMilliseconds(10));

        // Act
        _testFloor.TryAddPersonLocationUpdate(personMovementOffFloor);
        var personRemovedFromFloorEvent = await personRemovedFromFloorEventTask;

        // Assert
        personRemovedFromFloorEvent.Should().NotBeNull();
        personRemovedFromFloorEvent.FloorName.Should().Be(ValidFloorName);
        personRemovedFromFloorEvent.PersonId.Should().Be(personId);
    }

    [Fact]
    public async Task Dispose_ShouldNotRaisePersonExpiredEvent_WhenPersonIsLocatedOnFloor()
    {
        // Arrange
        var personId = Guid.NewGuid();
        var locationOnFloor = new Location(2, 2);
        var personEvents = new PersonEvents();
        _testFloor = new Floor(ValidFloorName, s_validOutline, personEvents, TimeSpan.FromMilliseconds(20));
        var personMovementOnFloor = new PersonLocationUpdate(personId, locationOnFloor);
        _testFloor.TryAddPersonLocationUpdate(personMovementOnFloor);

        var personExpiredEventTask =
            DomainEventsExtensions.RegisterAndWaitForEvent<PersonExpiredEvent>(
                h => personEvents.Expired += h,
                h => personEvents.Expired -= h,
                TimeSpan.FromMilliseconds(40));

        // Act
        _testFloor.Dispose();
        var personExpiredEvent = await personExpiredEventTask;

        // Assert
        personExpiredEvent.Should().BeNull();
    }

    public void Dispose()
    {
        _testFloor?.Dispose();
    }
}
