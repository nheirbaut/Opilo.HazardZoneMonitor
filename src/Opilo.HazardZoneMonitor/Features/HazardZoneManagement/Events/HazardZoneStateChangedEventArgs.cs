using Opilo.HazardZoneMonitor.Shared.Primitives;

namespace Opilo.HazardZoneMonitor.Features.HazardZoneManagement.Events;

public sealed class HazardZoneStateChangedEventArgs(
    string hazardZoneName,
    ZoneState newState) : EventArgs
{
    public string HazardZoneName { get; } = hazardZoneName;
    public ZoneState NewState { get; } = newState;
}
