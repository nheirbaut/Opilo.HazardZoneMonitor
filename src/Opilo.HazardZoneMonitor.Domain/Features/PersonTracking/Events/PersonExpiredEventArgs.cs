using Opilo.HazardZoneMonitor.Domain.Shared.ValueObjects;

namespace Opilo.HazardZoneMonitor.Domain.Features.PersonTracking.Events;

public sealed class PersonExpiredEventArgs(PersonId personId) : EventArgs
{
    public PersonId PersonId { get; } = personId;
}

