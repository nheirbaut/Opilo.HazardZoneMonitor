using System.Collections.ObjectModel;
using Opilo.HazardZoneMonitor.Domain.Features.HazardZoneManagement.Domain;
using Opilo.HazardZoneMonitor.Domain.Shared.Primitives;

namespace Opilo.HazardZoneMonitor.Domain.Tests.Unit.TestUtilities;

internal static class HazardZoneHelperExtensions
{
    public static Coordinate GetLocationOutside(this HazardZone hazardZone)
        => hazardZone.Outline.Vertices.GetPointOutside();

    public static Coordinate GetLocationInside(this HazardZone hazardZone)
        => hazardZone.Outline.Vertices.GetCentroid();

    public static Coordinate GetCentroid(this ReadOnlyCollection<Coordinate> locations)
    {
        ArgumentNullException.ThrowIfNull(locations);

        var sumX = 0.0;
        var sumY = 0.0;
        var count = 0;

        foreach (var location in locations)
        {
            sumX += location.X;
            sumY += location.Y;
            count++;
        }

        if (count == 0)
            throw new ArgumentException("The collection of locations cannot be empty.", nameof(locations));

        return new Coordinate(sumX / count, sumY / count);
    }

    public static Coordinate GetPointOutside(this ReadOnlyCollection<Coordinate> locations)
    {
        ArgumentNullException.ThrowIfNull(locations);

        if (locations.Count == 0)
            throw new ArgumentException("The collection of locations cannot be empty.", nameof(locations));

        var centroid = locations.GetCentroid();

        var farthestVertex = locations[0];
        var maxDistanceSquared = DistanceSquared(farthestVertex, centroid);

        foreach (var location in locations)
        {
            var distanceSquared = DistanceSquared(location, centroid);

            if (distanceSquared <= maxDistanceSquared)
                continue;

            maxDistanceSquared = distanceSquared;
            farthestVertex = location;
        }

        var directionX = farthestVertex.X - centroid.X;
        var directionY = farthestVertex.Y - centroid.Y;

        return new Coordinate(centroid.X + directionX * 2, centroid.Y + directionY * 2);
    }

    private static double DistanceSquared(Coordinate a, Coordinate b)
    {
        var dx = a.X - b.X;
        var dy = a.Y - b.Y;
        return dx * dx + dy * dy;
    }
}
