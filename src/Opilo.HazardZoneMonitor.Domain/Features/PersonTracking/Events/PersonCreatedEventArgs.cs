using Opilo.HazardZoneMonitor.Domain.Shared.Primitives;

namespace Opilo.HazardZoneMonitor.Domain.Features.PersonTracking.Events;

public sealed class PersonCreatedEventArgs(PersonId personId, Location location) : EventArgs
{
    public PersonId PersonId { get; } = personId;
    public Location Location { get; } = location;
}

