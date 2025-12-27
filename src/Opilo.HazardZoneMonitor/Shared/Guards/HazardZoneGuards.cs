using Ardalis.GuardClauses;
using Opilo.HazardZoneMonitor.Features.HazardZoneManagement.Domain;
using Opilo.HazardZoneMonitor.Shared.Primitives;

namespace Opilo.HazardZoneMonitor.Shared.Guards;

public static class HazardZoneGuards
{
    public static void HazardZonesOutsideFloor(
        this IGuardClause guardClause,
        IEnumerable<HazardZone> hazardZones,
        Outline floorOutline,
        string parameterName)
    {
        ArgumentNullException.ThrowIfNull(hazardZones);
        ArgumentNullException.ThrowIfNull(floorOutline);

        foreach (var hazardZone in hazardZones)
        {
            if (!hazardZone.Outline.IsWithin(floorOutline))
            {
                throw new ArgumentException(
                    $"HazardZone '{hazardZone.Name}' outline is not within Floor outline.",
                    parameterName);
            }
        }
    }
}
