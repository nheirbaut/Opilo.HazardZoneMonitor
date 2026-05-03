using Opilo.HazardZoneMonitor.Domain.Shared.Primitives;
using Opilo.HazardZoneMonitor.Domain.Shared.ValueObjects;

namespace Opilo.HazardZoneMonitor.Domain.Features.HazardZoneManagement.Domain;

public sealed class HazardZoneAlarmStateChangedEventArgs(HazardZoneName hazardZoneName, AlarmState newState) : EventArgs
{
    public HazardZoneName HazardZoneName { get; } = hazardZoneName;
    public AlarmState NewState { get; } = newState;
}
