using Opilo.HazardZoneMonitor.Domain.Shared.ValueObjects;

namespace Opilo.HazardZoneMonitor.Domain.Features.HazardZoneManagement.Events;

public sealed class PersonAddedToHazardZoneEventArgs(PersonId personId, HazardZoneName hazardZoneName) : EventArgs
{
    public PersonId PersonId { get; } = personId;
    public HazardZoneName HazardZoneName { get; } = hazardZoneName;
}
