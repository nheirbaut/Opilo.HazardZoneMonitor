using Opilo.HazardZoneMonitor.Shared.Primitives;

namespace Opilo.HazardZoneMonitor.Features.HazardZoneManagement.Domain;

public sealed class HazardZoneAlarmStateChangedEventArgs(string hazardZoneName, AlarmState newState) : EventArgs
{
    public string HazardZoneName { get; } = hazardZoneName;
    public AlarmState NewState { get; } = newState;
}
