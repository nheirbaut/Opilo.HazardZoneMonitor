using Opilo.HazardZoneMonitor.Domain.Shared.Primitives;

namespace Opilo.HazardZoneMonitor.Domain.Features.PersonTracking.Events;

public sealed class PersonLocationChangedEventArgs(Guid personId, Location currentLocation) : EventArgs
{
    public Guid PersonId { get; } = personId;
    public Location CurrentLocation { get; } = currentLocation;
}

