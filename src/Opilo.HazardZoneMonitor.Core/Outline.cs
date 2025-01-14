using System.Collections.ObjectModel;
using Ardalis.GuardClauses;

namespace Opilo.HazardZoneMonitor.Core;

public sealed class Outline
{
    private readonly ReadOnlyCollection<Location> _vertices;

    public Outline(ReadOnlyCollection<Location> vertices)
    {
        Guard.Against.Null(vertices);

        if (vertices.Count < 3)
            throw new ArgumentException("Outline must have at least 3 vertices.");

        _vertices = vertices;
    }

    // Based on the Winding Number Algorithm: https://en.wikipedia.org/wiki/Point_in_polygon#Winding_number_algorithm
    public bool IsLocationInside(Location location)
    {
        Guard.Against.Null(location);

        var windingNumber = 0;

        for (var i = 0; i < _vertices.Count; i++)
        {
            Location currentVertex = _vertices[i];
            Location nextVertex = _vertices[(i + 1) % _vertices.Count];

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
