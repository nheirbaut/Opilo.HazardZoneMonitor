namespace Opilo.HazardZoneMonitor.Features.PersonTracking.Events;

public sealed class PersonExpiredEventArgs(Guid personId) : EventArgs
{
    public Guid PersonId { get; } = personId;
}

