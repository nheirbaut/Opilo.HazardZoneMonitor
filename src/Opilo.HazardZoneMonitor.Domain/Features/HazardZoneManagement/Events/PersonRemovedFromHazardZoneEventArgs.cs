using Opilo.HazardZoneMonitor.Domain.Shared.ValueObjects;

namespace Opilo.HazardZoneMonitor.Domain.Features.HazardZoneManagement.Events;

public sealed class PersonRemovedFromHazardZoneEventArgs(PersonId personId, string hazardZoneName) : EventArgs
{
    public PersonId PersonId { get; } = personId;
    public string HazardZoneName { get; } = hazardZoneName;
}
