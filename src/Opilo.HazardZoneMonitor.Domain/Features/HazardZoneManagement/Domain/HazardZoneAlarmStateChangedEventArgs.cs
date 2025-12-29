using Opilo.HazardZoneMonitor.Domain.Shared.Primitives;

namespace Opilo.HazardZoneMonitor.Domain.Features.HazardZoneManagement.Domain;

public sealed class HazardZoneAlarmStateChangedEventArgs(string hazardZoneName, AlarmState newState) : EventArgs
{
    public string HazardZoneName { get; } = hazardZoneName;
    public AlarmState NewState { get; } = newState;
}
