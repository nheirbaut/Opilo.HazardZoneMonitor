using Opilo.HazardZoneMonitor.Domain.Shared.Primitives;

namespace Opilo.HazardZoneMonitor.Domain.Features.PersonTracking.Events;

public sealed class PersonCreatedEventArgs(PersonId personId, Coordinate location) : EventArgs
{
    public PersonId PersonId { get; } = personId;
    public Coordinate Location { get; } = location;
}

