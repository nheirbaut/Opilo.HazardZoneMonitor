using System.Collections.ObjectModel;
using Opilo.HazardZoneMonitor.Domain;
using Opilo.HazardZoneMonitor.Domain.Entities;
using Opilo.HazardZoneMonitor.Domain.ValueObjects;

namespace Opilo.HazardZoneMonitor.UnitTests.Domain;

public sealed class FloorTests
{
    private readonly Outline _validOutline = new(new ReadOnlyCollection<Location>([
        new Location(0, 0),
        new Location(4, 0),
        new Location(4, 4),
        new Location(0, 4)
    ]));

    [Fact]
    public void Constructor_ShouldThrowException_WhenNameIsNull()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new Floor(null!, _validOutline));
    }

    [Fact]
    public void Constructor_ShouldThrowException_WhenNameIsEmpty()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => new Floor(string.Empty, _validOutline));
    }

    [Fact]
    public void Constructor_ShouldThrowException_WhenNameIsWhitespace()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => new Floor("  ", _validOutline));
    }

    [Fact]
    public void Constructor_ShouldThrowException_WhenOutlineIsNull()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new Floor("Floor Name", null!));
    }

    [Fact]
    public void TryAddPersonLocationUpdate_ShouldThrowException_WhenPersonLocationIsNull()
    {
        // Arrange
        var floor = new Floor("Floor Name", _validOutline);

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => floor.TryAddPersonLocationUpdate(null!));
    }

    [Fact]
    public void TryAddPersonLocationUpdate_ShouldReturnFalse_WhenPersonLocationUpdateNotOnFloor()
    {
        // Arrange
        var floor = new Floor("Floor Name", _validOutline);
        var personMovement = new PersonLocationUpdate(Guid.NewGuid(), new Location(8, 8));

        // Act
        var result = floor.TryAddPersonLocationUpdate(personMovement);

        // Assert
        Assert.False(result);
    }
}
