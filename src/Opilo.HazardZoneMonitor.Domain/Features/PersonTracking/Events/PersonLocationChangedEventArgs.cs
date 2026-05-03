using Opilo.HazardZoneMonitor.Domain.Shared.Primitives;
using Opilo.HazardZoneMonitor.Domain.Shared.ValueObjects;

namespace Opilo.HazardZoneMonitor.Domain.Features.PersonTracking.Events;

public sealed class PersonLocationChangedEventArgs(PersonId personId, Location currentLocation) : EventArgs
{
    public PersonId PersonId { get; } = personId;
    public Location CurrentLocation { get; } = currentLocation;
}

