using Opilo.HazardZoneMonitor.Features.HazardZoneManagement.Domain;
using Opilo.HazardZoneMonitor.Features.PersonTracking.Events;

namespace Opilo.HazardZoneMonitor.Tests.Unit.TestUtilities;

internal static class PersonHelper
{
    public static PersonLocationChangedEventArgs CreatePersonLocationChangedEventLocatedInHazardZone(HazardZone hazardZone)
        => new(Guid.NewGuid(), hazardZone.Outline.Vertices.GetCentroid());
}
