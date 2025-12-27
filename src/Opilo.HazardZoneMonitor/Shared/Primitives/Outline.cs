using System.Collections.ObjectModel;
using Ardalis.GuardClauses;

namespace Opilo.HazardZoneMonitor.Shared.Primitives;

public sealed class Outline
{
    public ReadOnlyCollection<Location> Vertices { get; }

    public Outline(ReadOnlyCollection<Location> vertices)
    {
        Vertices = vertices;
        Guard.Against.Null(vertices);

        if (vertices.Count < 3)
            throw new ArgumentException("Outline must have at least 3 vertices.", nameof(vertices));

        Vertices = vertices;
    }

    // Based on the Winding Number Algorithm: https://en.wikipedia.org/wiki/Point_in_polygon#Winding_number_algorithm
    public bool IsLocationInside(Location location)
    {
        Guard.Against.Null(location);

        var windingNumber = 0;

        for (var i = 0; i < Vertices.Count; i++)
        {
            Location currentVertex = Vertices[i];
            Location nextVertex = Vertices[(i + 1) % Vertices.Count];

            if (IsUpwardCrossing(currentVertex, nextVertex, location))
            {
                if (IsPointLeftOfEdge(currentVertex, nextVertex, location))
                    windingNumber++;
            }
            else if (IsDownwardCrossing(currentVertex, nextVertex, location)
                     && IsPointRightOfEdge(currentVertex, nextVertex, location))
            {
                windingNumber--;
            }
        }

        return windingNumber != 0;
    }

    public bool Overlaps(Outline other)
    {
        Guard.Against.Null(other);

        if (Vertices.Any(v => other.IsLocationInside(v)))
            return true;

        if (other.Vertices.Any(v => IsLocationInside(v)))
            return true;

        return AnyEdgesIntersect(other);
    }

    public bool IsWithin(Outline other)
    {
        Guard.Against.Null(other);
        _ = Vertices;
        return false;
    }

    private bool AnyEdgesIntersect(Outline other)
    {
        for (var i = 0; i < Vertices.Count; i++)
        {
            var a1 = Vertices[i];
            var a2 = Vertices[(i + 1) % Vertices.Count];

            for (var j = 0; j < other.Vertices.Count; j++)
            {
                var b1 = other.Vertices[j];
                var b2 = other.Vertices[(j + 1) % other.Vertices.Count];

                if (EdgesIntersect(a1, a2, b1, b2))
                    return true;
            }
        }

        return false;
    }

    private static bool EdgesIntersect(Location a1, Location a2, Location b1, Location b2)
    {
        var d1 = CrossProduct(b2, b1, a1);
        var d2 = CrossProduct(b2, b1, a2);
        var d3 = CrossProduct(a2, a1, b1);
        var d4 = CrossProduct(a2, a1, b2);

        if (((d1 > 0 && d2 < 0) || (d1 < 0 && d2 > 0)) &&
            ((d3 > 0 && d4 < 0) || (d3 < 0 && d4 > 0)))
            return true;

        return false;
    }

    private static double CrossProduct(Location a, Location b, Location c)
        => (a.X - c.X) * (b.Y - c.Y) - (a.Y - c.Y) * (b.X - c.X);

    private static bool IsUpwardCrossing(Location v1, Location v2, Location point)
        => v1.Y <= point.Y && v2.Y > point.Y;

    private static bool IsDownwardCrossing(Location v1, Location v2, Location point)
        => v1.Y > point.Y && v2.Y <= point.Y;

    private static bool IsPointLeftOfEdge(Location v1, Location v2, Location point)
        => IsLeft(v1, v2, point) > 0;

    private static bool IsPointRightOfEdge(Location v1, Location v2, Location point)
        => IsLeft(v1, v2, point) < 0;

    private static double IsLeft(Location v1, Location v2, Location point)
        => (v2.X - v1.X) * (point.Y - v1.Y) - (point.X - v1.X) * (v2.Y - v1.Y);
}

