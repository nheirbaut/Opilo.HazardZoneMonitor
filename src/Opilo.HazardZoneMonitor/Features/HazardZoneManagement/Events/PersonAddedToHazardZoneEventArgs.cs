using Opilo.HazardZoneMonitor.Shared.Abstractions;

namespace Opilo.HazardZoneMonitor.Features.HazardZoneManagement.Events;

public sealed class PersonAddedToHazardZoneEventArgs(Guid personId, string hazardZoneName) : EventArgs
{
    public Guid PersonId { get; } = personId;
    public string HazardZoneName { get; } = hazardZoneName;
}
