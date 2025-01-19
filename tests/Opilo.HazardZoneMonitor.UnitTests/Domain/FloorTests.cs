using System.Collections.ObjectModel;
using Opilo.HazardZoneMonitor.Domain.Entities;
using Opilo.HazardZoneMonitor.Domain.Events;
using Opilo.HazardZoneMonitor.Domain.Services;
using Opilo.HazardZoneMonitor.Domain.ValueObjects;
using Opilo.HazardZoneMonitor.UnitTests.TestUtilities;

namespace Opilo.HazardZoneMonitor.UnitTests.Domain;

public sealed class FloorTests : IDisposable
{
    private readonly Outline _validOutline = new(new ReadOnlyCollection<Location>([
        new Location(0, 0),
        new Location(4, 0),
        new Location(4, 4),
        new Location(0, 4)
    ]));

    [Fact]
    public void Constructor_WhenNameIsNull_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new Floor(null!, _validOutline));
    }

    [Fact]
    public void Constructor_WhenNameIsEmpty_ThrowsArgumentException()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => new Floor(string.Empty, _validOutline));
    }

    [Fact]
    public void Constructor_WhenNameIsWhitespace_ThrowsArgumentException()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => new Floor("  ", _validOutline));
    }

    [Fact]
    public void Constructor_WhenOutlineIsNull_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new Floor("Floor Name", null!));
    }

    [Fact]
    public void TryAddPersonLocationUpdate_WhenPersonLocationIsNull_ThrowsArgumentNullException()
    {
        // Arrange
        var floor = new Floor("Floor Name", _validOutline);

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => floor.TryAddPersonLocationUpdate(null!));
    }

    [Fact]
    public void TryAddPersonLocationUpdate_WhenPersonLocationUpdateNotOnFloor_ReturnsFalse()
    {
        // Arrange
        var floor = new Floor("Floor Name", _validOutline);
        var personMovement = new PersonLocationUpdate(Guid.NewGuid(), new Location(8, 8));

        // Act
        var result = floor.TryAddPersonLocationUpdate(personMovement);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void TryAddPersonLocationUpdate_WhenPersonLocationUpdateOnFloor_ReturnsTrue()
    {
        // Arrange
        var floor = new Floor("Floor Name", _validOutline);
        var personMovement = new PersonLocationUpdate(Guid.NewGuid(), new Location(2, 2));

        // Act
        var result = floor.TryAddPersonLocationUpdate(personMovement);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task TryAddPersonLocationUpdate_WhenPersonLocationUpdateOnFloorAndNewPerson_RaisesPersonAddedToFloorEvent()
    {
        // Arrange
        const string floorName = "Floor Name";
        var personId = Guid.NewGuid();
        var location = new Location(2, 2);
        var floor = new Floor(floorName, _validOutline);
        var personMovement = new PersonLocationUpdate(personId, location);
        var personAddedToFloorEventTask = DomainEventsExtensions.RegisterAndWaitForEvent<PersonAddedToFloorEvent>();

        // Act
        floor.TryAddPersonLocationUpdate(personMovement);
        var personAddedToFloorEvent = await personAddedToFloorEventTask;

        // Assert
        Assert.NotNull(personAddedToFloorEvent);
        Assert.Equal(floorName, personAddedToFloorEvent.FloorName);
        Assert.Equal(personId, personAddedToFloorEvent.PersonId);
        Assert.Equal(location, personAddedToFloorEvent.Location);
    }

    [Fact]
    public void TryAddPersonLocationUpdate_WhenPersonLocationUpdateOnFloorAndKnownPerson_DoesNotRaisePersonAddedToFloorEvent()
    {
        // Arrange
        const string floorName = "Floor Name";
        var personId = Guid.NewGuid();
        var location = new Location(2, 2);
        var floor = new Floor(floorName, _validOutline);
        var personMovement = new PersonLocationUpdate(personId, location);
        PersonAddedToFloorEvent? personAddedToFloorEvent = null;
        floor.TryAddPersonLocationUpdate(personMovement);

        DomainEvents.Register<PersonAddedToFloorEvent>(e => personAddedToFloorEvent = e);

        // Act
        floor.TryAddPersonLocationUpdate(personMovement);

        // Assert
        Assert.Null(personAddedToFloorEvent);
    }

    public void Dispose()
    {
        DomainEvents.Dispose();
    }
}
