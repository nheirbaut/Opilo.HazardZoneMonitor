using Opilo.HazardZoneMonitor.Domain.Shared.Primitives;

namespace Opilo.HazardZoneMonitor.Domain.Features.PersonTracking.Events;

public sealed class PersonCreatedEventArgs(Guid personId, Location location) : EventArgs
{
    public Guid PersonId { get; } = personId;
    public Location Location { get; } = location;
}

