using Opilo.HazardZoneMonitor.Domain.Shared.Primitives;
using Opilo.HazardZoneMonitor.Domain.Shared.ValueObjects;

namespace Opilo.HazardZoneMonitor.Domain.Features.HazardZoneManagement.Events;

public sealed class HazardZoneStateChangedEventArgs(
    HazardZoneName hazardZoneName,
    ZoneState newState) : EventArgs
{
    public HazardZoneName HazardZoneName { get; } = hazardZoneName;
    public ZoneState NewState { get; } = newState;
}
