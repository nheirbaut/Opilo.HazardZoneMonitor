using Opilo.HazardZoneMonitor.Domain.Entities;
using Opilo.HazardZoneMonitor.Domain.Events.FloorEvents;
using Opilo.HazardZoneMonitor.Domain.Events.PersonEvents;
using Opilo.HazardZoneMonitor.Domain.Services;
using Opilo.HazardZoneMonitor.Domain.ValueObjects;
using Opilo.HazardZoneMonitor.UnitTests.TestUtilities;

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
    public void Constructor_WhenNameIsNull_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new Floor(null!, s_validOutline));
    }

    [Theory]
    [ClassData(typeof(InvalidNames))]
    public void Constructor_WhenNameIsInvalid_ThrowsArgumentException(string invalidName)
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => new Floor(invalidName, s_validOutline));
    }

    [Fact]
    public void Constructor_WhenOutlineIsNull_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new Floor(ValidFloorName, null!));
    }

    [Fact]
    public void Constructor_WhenValidNameAndOutlineGiven_CreatesInstance()
    {
        // Act
        _testFloor = new Floor(ValidFloorName, s_validOutline);

        // Assert
        Assert.Equal(ValidFloorName, _testFloor.Name);
        Assert.Equal(s_validOutline, _testFloor.Outline);
    }

    [Fact]
    public void TryAddPersonLocationUpdate_WhenPersonLocationUpdateIsNull_ThrowsArgumentNullException()
    {
        // Arrange
        _testFloor = new Floor(ValidFloorName, s_validOutline);

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => _testFloor.TryAddPersonLocationUpdate(null!));
    }

    [Fact]
    public void TryAddPersonLocationUpdate_WhenPersonLocationUpdateNotOnFloor_ReturnsFalse()
    {
        // Arrange
        _testFloor = new Floor(ValidFloorName, s_validOutline);
        var personMovement = new PersonLocationUpdate(Guid.NewGuid(), new Location(8, 8));

        // Act
        var result = _testFloor.TryAddPersonLocationUpdate(personMovement);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void TryAddPersonLocationUpdate_WhenPersonLocationUpdateOnFloor_ReturnsTrue()
    {
        // Arrange
        _testFloor = new Floor(ValidFloorName, s_validOutline);
        var personMovement = new PersonLocationUpdate(Guid.NewGuid(), new Location(2, 2));

        // Act
        var result = _testFloor.TryAddPersonLocationUpdate(personMovement);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task
        TryAddPersonLocationUpdate_WhenPersonLocationUpdateOnFloorAndNewPerson_RaisesPersonAddedToFloorEvent()
    {
        // Arrange
        var personId = Guid.NewGuid();
        var location = new Location(2, 2);
        _testFloor = new Floor(ValidFloorName, s_validOutline);
        var personMovement = new PersonLocationUpdate(personId, location);
        var personAddedToFloorEventTask = DomainEventsExtensions.RegisterAndWaitForEvent<PersonAddedToFloorEvent>();

        // Act
        _testFloor.TryAddPersonLocationUpdate(personMovement);
        var personAddedToFloorEvent = await personAddedToFloorEventTask;

        // Assert
        Assert.NotNull(personAddedToFloorEvent);
        Assert.Equal(ValidFloorName, personAddedToFloorEvent.FloorName);
        Assert.Equal(personId, personAddedToFloorEvent.PersonId);
        Assert.Equal(location, personAddedToFloorEvent.Location);
    }

    [Fact]
    public async Task
        TryAddPersonLocationUpdate_WhenPersonLocationUpdateOnFloorAndKnownPerson_DoesNotRaisePersonAddedToFloorEvent()
    {
        // Arrange
        var personId = Guid.NewGuid();
        var location = new Location(2, 2);
        _testFloor = new Floor(ValidFloorName, s_validOutline);
        var personMovement = new PersonLocationUpdate(personId, location);
        _testFloor.TryAddPersonLocationUpdate(personMovement);

        var personAddedToFloorEventTask = DomainEventsExtensions.RegisterAndWaitForEvent<PersonAddedToFloorEvent>(TimeSpan.FromMilliseconds(50));

        // Act
        _testFloor.TryAddPersonLocationUpdate(personMovement);
        var personAddedToFloorEvent = await personAddedToFloorEventTask;

        // Assert
        Assert.Null(personAddedToFloorEvent);
    }

    [Fact]
    public async Task TryAddPersonLocationUpdate_WhenPersonExpires_RaisesPersonRemovedFromFloorEvent()
    {
        // Arrange
        var personId = Guid.NewGuid();
        var location = new Location(2, 2);
        _testFloor = new Floor(ValidFloorName, s_validOutline, TimeSpan.FromMilliseconds(10));
        var personMovement = new PersonLocationUpdate(personId, location);

        var personRemovedFromFloorEventTask = DomainEventsExtensions.RegisterAndWaitForEvent<PersonRemovedFromFloorEvent>();

        // Act
        _testFloor.TryAddPersonLocationUpdate(personMovement);
        var personRemovedFromFloorEvent = await personRemovedFromFloorEventTask;

        // Assert
        Assert.NotNull(personRemovedFromFloorEvent);
        Assert.Equal(ValidFloorName, personRemovedFromFloorEvent.FloorName);
        Assert.Equal(personId, personRemovedFromFloorEvent.PersonId);
    }

    [Fact]
    public async Task
        TryAddPersonLocationUpdate_WhenPersonLocationUpdateOffFloorAndKnownPerson_RaisesPersonRemovedFromFloorEvent()
    {
        // Arrange
        var personId = Guid.NewGuid();
        var locationOnFloor = new Location(2, 2);
        var locationOffFloor = new Location(200, 200);
        _testFloor = new Floor(ValidFloorName, s_validOutline);
        var personMovementOnFloor = new PersonLocationUpdate(personId, locationOnFloor);
        var personMovementOffFloor = new PersonLocationUpdate(personId, locationOffFloor);
        _testFloor.TryAddPersonLocationUpdate(personMovementOnFloor);

        var personRemovedFromFloorEventTask = DomainEventsExtensions.RegisterAndWaitForEvent<PersonRemovedFromFloorEvent>(TimeSpan.FromMilliseconds(10));

        // Act
        _testFloor.TryAddPersonLocationUpdate(personMovementOffFloor);
        var personRemovedFromFloorEvent = await personRemovedFromFloorEventTask;

        // Assert
        Assert.NotNull(personRemovedFromFloorEvent);
        Assert.Equal(ValidFloorName, personRemovedFromFloorEvent.FloorName);
        Assert.Equal(personId, personRemovedFromFloorEvent.PersonId);
    }

    [Fact]
    public async Task Dispose_WhenPersonLocatedOnFloor_DoesNotRaisePersonExpiredEvent()
    {
        // Arrange
        var personId = Guid.NewGuid();
        var locationOnFloor = new Location(2, 2);
        _testFloor = new Floor(ValidFloorName, s_validOutline, TimeSpan.FromMilliseconds(20));
        var personMovementOnFloor = new PersonLocationUpdate(personId, locationOnFloor);
        _testFloor.TryAddPersonLocationUpdate(personMovementOnFloor);

        var personExpiredEventTask =
            DomainEventsExtensions.RegisterAndWaitForEvent<PersonExpiredEvent>(TimeSpan.FromMilliseconds(40));

        // Act
        _testFloor.Dispose();
        var personExpiredEvent = await personExpiredEventTask;

        // Assert
        Assert.Null(personExpiredEvent);
    }

    public void Dispose()
    {
        DomainEvents.Dispose();
        _testFloor?.Dispose();
    }
}
