using Opilo.HazardZoneMonitor.Domain.Features.HazardZoneManagement.Domain;

namespace Opilo.HazardZoneMonitor.Api.Features.HazardZones;

public static class HazardZoneExtensions
{
    public static HazardZoneInfo ToHazardZoneInfo(this HazardZone hazardZone)
    {
        return new HazardZoneInfo(
            hazardZone.Name,
            hazardZone.Outline.Vertices,
            hazardZone.ActivationDuration.Value,
            hazardZone.PreAlarmDuration.Value,
            hazardZone.AllowedNumberOfPersons.Value,
            hazardZone.ZoneState,
            hazardZone.AlarmState);
    }
}
