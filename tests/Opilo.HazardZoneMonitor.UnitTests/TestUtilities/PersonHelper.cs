using System.Collections.ObjectModel;
using Opilo.HazardZoneMonitor.Entities;
using Opilo.HazardZoneMonitor.Events.PersonEvents;
using Opilo.HazardZoneMonitor.Shared.Primitives;

namespace Opilo.HazardZoneMonitor.UnitTests.TestUtilities;

internal static class PersonHelper
{
    public static PersonCreatedEvent CreatePersonCreatedEventLocatedInHazardZone(HazardZone hazardZone)
        => new(Guid.NewGuid(), hazardZone.Outline.Vertices.GetCentroid());

    public static PersonCreatedEvent CreatePersonCreatedEventLocatedOutsideHazardZone(HazardZone hazardZone)
        => new(Guid.NewGuid(), hazardZone.Outline.Vertices.GetPointOutside());

    public static PersonLocationChangedEvent CreatePersonLocationChangedEventLocatedInHazardZone(HazardZone hazardZone)
        => new(Guid.NewGuid(), hazardZone.Outline.Vertices.GetCentroid());

    public static PersonLocationChangedEvent CreatePersonLocationChangedEventLocatedOutsideHazardZone(HazardZone hazardZone)
        => new(Guid.NewGuid(), hazardZone.Outline.Vertices.GetPointOutside());

    public static PersonLocationChangedEvent CreatePersonLocationChangedEventLocatedOutsideHazardZone(HazardZone hazardZone, Guid personId)
        => new(personId, hazardZone.Outline.Vertices.GetPointOutside());

    private static Location GetCentroid(this ReadOnlyCollection<Location> locations)
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

        return new Location(sumX / count, sumY / count);
    }

    private static Location GetPointOutside(this ReadOnlyCollection<Location> locations)
    {
        ArgumentNullException.ThrowIfNull(locations);

        if (locations.Count == 0)
            throw new ArgumentException("The collection of locations cannot be empty.", nameof(locations));

        var centroid = locations.GetCentroid();

        var farthestVertex = locations[0];
        var maxDistanceSquared = DistanceSquared(farthestVertex, centroid);

        foreach (var location in locations)
        {
            double distanceSquared = DistanceSquared(location, centroid);

            if (distanceSquared <= maxDistanceSquared)
                continue;

            maxDistanceSquared = distanceSquared;
            farthestVertex = location;
        }

        var directionX = farthestVertex.X - centroid.X;
        var directionY = farthestVertex.Y - centroid.Y;

        return new Location(centroid.X + directionX * 2, centroid.Y + directionY * 2);
    }

    private static double DistanceSquared(Location a, Location b)
    {
        var dx = a.X - b.X;
        var dy = a.Y - b.Y;
        return dx * dx + dy * dy;
    }
}
