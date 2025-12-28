using Opilo.HazardZoneMonitor.Features.HazardZoneManagement.Domain;
using Opilo.HazardZoneMonitor.Features.PersonTracking.Events;

namespace Opilo.HazardZoneMonitor.Tests.Unit.TestUtilities;

internal static class PersonHelper
{
    public static PersonCreatedEventArgs CreatePersonCreatedEventLocatedOutsideHazardZone(HazardZone hazardZone)
        => new(Guid.NewGuid(), hazardZone.Outline.Vertices.GetPointOutside());

    public static PersonLocationChangedEventArgs CreatePersonLocationChangedEventLocatedInHazardZone(HazardZone hazardZone)
        => new(Guid.NewGuid(), hazardZone.Outline.Vertices.GetCentroid());

    public static PersonLocationChangedEventArgs CreatePersonLocationChangedEventLocatedOutsideHazardZone(HazardZone hazardZone)
        => new(Guid.NewGuid(), hazardZone.Outline.Vertices.GetPointOutside());

    public static PersonLocationChangedEventArgs CreatePersonLocationChangedEventLocatedOutsideHazardZone(HazardZone hazardZone, Guid personId)
        => new(personId, hazardZone.Outline.Vertices.GetPointOutside());
}
