namespace Opilo.HazardZoneMonitor.Features.HazardZoneManagement.Events;

public class HazardZoneActivatedEventArgs(string hazardZoneName) : EventArgs
{
    public string HazardZoneName { get; } = hazardZoneName;
}
