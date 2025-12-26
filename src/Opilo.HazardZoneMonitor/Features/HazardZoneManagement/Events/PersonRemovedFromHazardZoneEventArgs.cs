namespace Opilo.HazardZoneMonitor.Features.HazardZoneManagement.Events;

public sealed class PersonRemovedFromHazardZoneEventArgs(Guid personId, string hazardZoneName) : EventArgs
{
    public Guid PersonId { get; } = personId;
    public string HazardZoneName { get; } = hazardZoneName;
}
