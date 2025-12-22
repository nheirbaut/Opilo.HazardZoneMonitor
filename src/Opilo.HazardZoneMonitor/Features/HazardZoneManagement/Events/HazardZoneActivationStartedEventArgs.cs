using Opilo.HazardZoneMonitor.Shared.Abstractions;

namespace Opilo.HazardZoneMonitor.Features.HazardZoneManagement.Events;

public class HazardZoneActivationStartedEventArgs(string hazardZoneName) : EventArgs
{
    public string HazardZoneName { get; } = hazardZoneName;
}
