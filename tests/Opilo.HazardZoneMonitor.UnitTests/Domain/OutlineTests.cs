using System.Collections.ObjectModel;
using Opilo.HazardZoneMonitor.Shared.Primitives;

namespace Opilo.HazardZoneMonitor.UnitTests.Domain;

public sealed class OutlineTests
{
    private static readonly ReadOnlyCollection<Location> s_validVertices = new([
        new Location(0, 0),
        new Location(4, 0),
        new Location(4, 4),
        new Location(0, 4)
    ]);

    [Fact]
    public void Constructor_WhenVerticesAreNull_ThrowsArgumentNullException()
    {
        // Act & Assert
        var act = () => new Outline(null!);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Constructor_WhenVerticesAreLessThanThree_ThrowsArgumentException()
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
    public void COnstructor_WhenVerticesAreValid_CreatesInstance()
    {
        // Act
        var outline = new Outline(s_validVertices);

        // Assert
        outline.Vertices.Should().Equal(s_validVertices);
    }

    [Fact]
    public void IsLocationInside_WhenLocationIsNull_ThrowsArgumentNullException()
    {
        // Arrange
        var outline = new Outline(s_validVertices);

        // Act & Assert
        var act = () => outline.IsLocationInside(null!);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void IsLocationInside_WhenPointIsInsidePolygon_ReturnsTrue()
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
    public void IsLocationInside_WhenPointIsOutsidePolygon_ReturnsFalse()
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
    public void IsLocationInside_WhenPointIsOnPolygonEdge_ReturnsTrue()
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
    public void IsLocationInside_ForConcavePolygons_ReturnsTrueForPointsInsideAndFalseForPointsOutside()
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
}
