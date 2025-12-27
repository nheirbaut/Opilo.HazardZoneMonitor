using System.Collections.ObjectModel;
using Opilo.HazardZoneMonitor.Shared.Primitives;

namespace Opilo.HazardZoneMonitor.Tests.Unit.Domain;

public sealed class OutlineTests
{
    private static readonly ReadOnlyCollection<Location> s_validVertices = new([
        new Location(0, 0),
        new Location(4, 0),
        new Location(4, 4),
        new Location(0, 4)
    ]);

    [Fact]
    public void Constructor_ShouldThrowArgumentNullException_WhenVerticesAreNull()
    {
        // Act & Assert
        var act = () => new Outline(null!);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Constructor_ShouldThrowArgumentException_WhenVerticesAreLessThanThree()
    {
        // Arrange
        var vertices = new ReadOnlyCollection<Location>([
            new Location(0, 0),
            new Location(1, 1)]);

        // Act & Assert
        var act = () => new Outline(vertices);
        act.Should().Throw<ArgumentException>()
            .Where(ex => ex.Message.Contains("at least 3 vertices", StringComparison.Ordinal));
    }

    [Fact]
    public void Constructor_ShouldCreateInstance_WhenVerticesAreValid()
    {
        // Act
        var outline = new Outline(s_validVertices);

        // Assert
        outline.Vertices.Should().Equal(s_validVertices);
    }

    [Fact]
    public void IsLocationInside_ShouldThrowArgumentNullException_WhenLocationIsNull()
    {
        // Arrange
        var outline = new Outline(s_validVertices);

        // Act & Assert
        var act = () => outline.IsLocationInside(null!);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void IsLocationInside_ShouldReturnTrue_WhenLocationIsInsidePolygon()
    {
        // Arrange
        var outline = new Outline(s_validVertices);
        var point = new Location(2, 2);

        // Act
        var result = outline.IsLocationInside(point);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsLocationInside_ShouldReturnFalse_WhenLocationIsOutsidePolygon()
    {
        // Arrange
        var outline = new Outline(s_validVertices);
        var point = new Location(5, 5);

        // Act
        var result = outline.IsLocationInside(point);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void IsLocationInside_ShouldReturnTrue_WhenLocationIsOnPolygonEdge()
    {
        // Arrange
        var outline = new Outline(s_validVertices);
        var point = new Location(2, 0);

        // Act
        var result = outline.IsLocationInside(point);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsLocationInside_ShouldReturnTrueForInsidePointsAndFalseForOutsidePoints_WhenPolygonIsConcave()
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
        resultInside.Should().BeTrue();
        resultOutside.Should().BeFalse();
    }

    [Fact]
    public void Overlaps_ShouldThrowArgumentNullException_WhenOtherIsNull()
    {
        // Arrange
        var outline = new Outline(s_validVertices);

        // Act
        var act = () => outline.Overlaps(null!);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Overlaps_ShouldReturnTrue_WhenOutlinesShareEdgeIntersection()
    {
        // Arrange
        var outline1 = new Outline(new ReadOnlyCollection<Location>([
            new Location(0, 0),
            new Location(4, 0),
            new Location(4, 4),
            new Location(0, 4)
        ]));
        var outline2 = new Outline(new ReadOnlyCollection<Location>([
            new Location(2, 2),
            new Location(6, 2),
            new Location(6, 6),
            new Location(2, 6)
        ]));

        // Act
        var result = outline1.Overlaps(outline2);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void Overlaps_ShouldReturnFalse_WhenOutlinesAreCompletelyDisjoint()
    {
        // Arrange
        var outline1 = new Outline(new ReadOnlyCollection<Location>([
            new Location(0, 0),
            new Location(2, 0),
            new Location(2, 2),
            new Location(0, 2)
        ]));
        var outline2 = new Outline(new ReadOnlyCollection<Location>([
            new Location(5, 5),
            new Location(7, 5),
            new Location(7, 7),
            new Location(5, 7)
        ]));

        // Act
        var result = outline1.Overlaps(outline2);

        // Assert
        result.Should().BeFalse();
    }
}
