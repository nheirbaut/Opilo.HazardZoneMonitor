using Opilo.HazardZoneMonitor.Domain.Features.HazardZoneManagement.Domain;
using Opilo.HazardZoneMonitor.Domain.Features.PersonTracking.Events;

namespace Opilo.HazardZoneMonitor.Domain.Tests.Unit.TestUtilities;

internal static class PersonHelper
{
    public static PersonLocationChangedEventArgs CreatePersonLocationChangedEventLocatedInHazardZone(HazardZone hazardZone)
        => new(Guid.NewGuid(), hazardZone.Outline.Vertices.GetCentroid());
}
