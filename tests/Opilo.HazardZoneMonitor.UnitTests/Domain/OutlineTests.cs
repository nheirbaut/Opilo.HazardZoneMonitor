using System.Collections.ObjectModel;
using Opilo.HazardZoneMonitor.Domain.ValueObjects;

namespace Opilo.HazardZoneMonitor.UnitTests.Domain;

public sealed class OutlineTests
{
    private readonly ReadOnlyCollection<Location> _validVertices = new([
        new Location(0, 0),
        new Location(4, 0),
        new Location(4, 4),
        new Location(0, 4)
    ]);

    [Fact]
    public void Constructor_ShouldThrowException_WhenVerticesAreNull()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new Outline(null!));
    }

    [Fact]
    public void Constructor_ShouldThrowException_WhenVerticesAreLessThanThree()
    {
        // Arrange
        var vertices = new ReadOnlyCollection<Location>([
            new Location(0, 0),
            new Location(1, 1)]);

        // Act & Assert
        var ex = Assert.Throws<ArgumentException>(() => new Outline(vertices));
        Assert.Equal("Outline must have at least 3 vertices.", ex.Message);
    }

    [Fact]
    public void IsLocationInside_ShouldThrowException_WhenLocationIsNull()
    {
        // Arrange
        var outline = new Outline(_validVertices);

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => outline.IsLocationInside(null!));
    }

    [Fact]
    public void IsLocationInside_ShouldReturnTrue_WhenPointIsInsidePolygon()
    {
        // Arrange
        var outline = new Outline(_validVertices);
        var point = new Location(2, 2);

        // Act
        var result = outline.IsLocationInside(point);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void IsLocationInside_ShouldReturnFalse_WhenPointIsOutsidePolygon()
    {
        // Arrange
        var outline = new Outline(_validVertices);
        var point = new Location(5, 5);

        // Act
        var result = outline.IsLocationInside(point);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void IsLocationInside_ShouldReturnTrue_WhenPointIsOnPolygonEdge()
    {
        // Arrange
        var outline = new Outline(_validVertices);
        var point = new Location(2, 0);

        // Act
        var result = outline.IsLocationInside(point);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void IsLocationInside_ShouldWorkForConcavePolygons()
    {
        // Arrange
        var vertices = new ReadOnlyCollection<Location>([
            new Location(0, 0),
            new Location(4, 0),
            new Location(2, 2),
            new Location(4, 4),
            new Location(0, 4)
        ]);
        var outline = new Outline(vertices);
        var pointInside = new Location(2, 1);
        var pointOutside = new Location(3, 3);

        // Act
        var resultInside = outline.IsLocationInside(pointInside);
        var resultOutside = outline.IsLocationInside(pointOutside);

        // Assert
        Assert.True(resultInside);
        Assert.False(resultOutside);
    }
}
