using Opilo.HazardZoneMonitor.Domain.Shared.Primitives;

namespace Opilo.HazardZoneMonitor.Domain.Features.PersonTracking.Events;

public sealed class PersonExpiredEventArgs(PersonId personId) : EventArgs
{
    public PersonId PersonId { get; } = personId;
}

