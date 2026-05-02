using System.Collections.ObjectModel;
using Microsoft.Extensions.Options;
using Opilo.HazardZoneMonitor.Api.Features.HazardZones;
using Opilo.HazardZoneMonitor.Domain.Shared.Primitives;

namespace Opilo.HazardZoneMonitor.Api.Features.Floors;

public sealed class FloorOptionsValidator : IValidateOptions<FloorOptions>
{
    public ValidateOptionsResult Validate(string? name, FloorOptions options)
    {
        if (options.Floors.Any(floor => string.IsNullOrWhiteSpace(floor.Name)))
        {
            return ValidateOptionsResult.Fail("Each floor must have a non-empty name.");
        }

        if (options.Floors.Select(floor => floor.Name).Distinct(StringComparer.OrdinalIgnoreCase).Count() != options.Floors.Count)
        {
            return ValidateOptionsResult.Fail("Floor names must be unique (case-insensitive).");
        }

        if (options.Floors.Any(floor => floor.Outline.Count < 3))
        {
            return ValidateOptionsResult.Fail("Each floor's outline must have at least 3 points.");
        }

        var hazardZoneValidator = new HazardZoneOptionsValidator();

        foreach (var floor in options.Floors)
        {
            var hazardZoneOptions = new HazardZoneOptions { HazardZones = floor.HazardZones };
            var result = hazardZoneValidator.Validate(null, hazardZoneOptions);
            if (!result.Succeeded)
            {
                return ValidateOptionsResult.Fail($"Floor '{floor.Name}': {string.Join(", ", result.Failures ?? [])}");
            }
        }

        foreach (var floor in options.Floors)
        {
            var hazardZoneList = floor.HazardZones.ToList();
            for (var i = 0; i < hazardZoneList.Count; i++)
            {
                for (var j = i + 1; j < hazardZoneList.Count; j++)
                {
                    var outline1 = ToOutline(hazardZoneList[i].Outline);
                    var outline2 = ToOutline(hazardZoneList[j].Outline);
                    if (outline1.Overlaps(outline2))
                    {
                        return ValidateOptionsResult.Fail($"HazardZone '{hazardZoneList[i].Name}' overlaps with '{hazardZoneList[j].Name}'.");
                    }
                }
            }
        }

        foreach (var floor in options.Floors)
        {
            var floorOutline = ToOutline(floor.Outline);
            foreach (var hazardZone in floor.HazardZones)
            {
                var hazardZoneOutline = ToOutline(hazardZone.Outline);
                if (!hazardZoneOutline.IsWithin(floorOutline))
                {
                    return ValidateOptionsResult.Fail($"HazardZone '{hazardZone.Name}' is not within floor '{floor.Name}'.");
                }
            }
        }

        return ValidateOptionsResult.Success;
    }

    private static Outline ToOutline(IReadOnlyList<Shared.Configuration.PointConfiguration> points)
    {
        var locations = points.Select(p => new Location(p.X, p.Y)).ToList();
        return new Outline(new ReadOnlyCollection<Location>(locations));
    }
}
