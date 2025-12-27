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
            if (hazardZone is null)
            {
                throw new ArgumentException("Collection contains null HazardZone elements.", parameterName);
            }

            if (!hazardZone.Outline.IsWithin(floorOutline))
            {
                throw new ArgumentException(
                    $"HazardZone '{hazardZone.Name}' outline is not within Floor outline.",
                    parameterName);
            }
        }
    }

    public static void DuplicateHazardZones(
        this IGuardClause guardClause,
        IReadOnlyCollection<HazardZone> hazardZones,
        string parameterName)
    {
        ArgumentNullException.ThrowIfNull(hazardZones);

        var names = hazardZones.Select(hz => hz.Name).ToList();
        var distinctNames = names.Distinct(StringComparer.OrdinalIgnoreCase).Count();
        if (distinctNames != names.Count)
        {
            throw new ArgumentException("Duplicate HazardZone names are not allowed.", parameterName);
        }
    }

    public static void OverlappingHazardZones(
        this IGuardClause guardClause,
        IReadOnlyCollection<HazardZone> hazardZones,
        string parameterName)
    {
        ArgumentNullException.ThrowIfNull(hazardZones);

        var hazardZoneList = hazardZones.ToList();

        for (var i = 0; i < hazardZoneList.Count; i++)
        {
            for (var j = i + 1; j < hazardZoneList.Count; j++)
            {
                if (hazardZoneList[i].Outline.Overlaps(hazardZoneList[j].Outline))
                {
                    throw new ArgumentException(
                        $"HazardZone '{hazardZoneList[i].Name}' overlaps with '{hazardZoneList[j].Name}'.",
                        parameterName);
                }
            }
        }
    }
}
