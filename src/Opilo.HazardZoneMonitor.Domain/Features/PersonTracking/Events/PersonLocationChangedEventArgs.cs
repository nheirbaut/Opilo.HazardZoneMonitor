using Opilo.HazardZoneMonitor.Domain.Shared.Primitives;

namespace Opilo.HazardZoneMonitor.Domain.Features.PersonTracking.Events;

public sealed class PersonLocationChangedEventArgs(PersonId personId, Coordinate currentLocation) : EventArgs
{
    public PersonId PersonId { get; } = personId;
    public Coordinate CurrentLocation { get; } = currentLocation;
}

