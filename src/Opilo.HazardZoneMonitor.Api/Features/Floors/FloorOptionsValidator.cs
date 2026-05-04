using System.Collections.ObjectModel;
using Microsoft.Extensions.Options;
using Opilo.HazardZoneMonitor.Api.Features.HazardZones;
using Opilo.HazardZoneMonitor.Domain.Shared.Primitives;

namespace Opilo.HazardZoneMonitor.Api.Features.Floors;

public sealed class FloorOptionsValidator : IValidateOptions<FloorOptions>
{
    public ValidateOptionsResult Validate(string? name, FloorOptions options)
    {
        if (ReferenceEquals(options.Floors, null))
        {
            return ValidateOptionsResult.Fail("Floors configuration is missing.");
        }

        var result = ValidateFloorNamesAreUnique(options.Floors);
        if (!result.Succeeded) return result;

        result = ValidateFloorOutlinesHaveMinimumPoints(options.Floors);
        if (!result.Succeeded) return result;

        result = ValidateHazardZones(options.Floors);
        if (!result.Succeeded) return result;

        result = ValidateHazardZonesDoNotOverlap(options.Floors);
        if (!result.Succeeded) return result;

        result = ValidateHazardZonesAreWithinFloorOutline(options.Floors);
        if (!result.Succeeded) return result;

        return ValidateOptionsResult.Success;
    }

    private static ValidateOptionsResult ValidateFloorNamesAreUnique(IReadOnlyList<FloorConfiguration> floors)
    {
        var distinctCount = floors.Select(floor => floor.Name).Distinct().Count();
        if (distinctCount != floors.Count)
        {
            return ValidateOptionsResult.Fail("Floor names must be unique (case-insensitive).");
        }

        return ValidateOptionsResult.Success;
    }

    private static ValidateOptionsResult ValidateFloorOutlinesHaveMinimumPoints(IReadOnlyList<FloorConfiguration> floors)
    {
        var invalidFloor = floors.FirstOrDefault(HasInvalidOutline);
        if (invalidFloor is not null)
        {
            if (ReferenceEquals(invalidFloor.Outline, null))
            {
                return ValidateOptionsResult.Fail("Each floor must have an outline.");
            }

            return ValidateOptionsResult.Fail("Each floor's outline must have at least 3 points.");
        }

        return ValidateOptionsResult.Success;
    }

    private static bool HasInvalidOutline(FloorConfiguration floor)
    {
        if (ReferenceEquals(floor.Outline, null))
        {
            return true;
        }

        return floor.Outline.Count < 3;
    }

    private static ValidateOptionsResult ValidateHazardZones(IReadOnlyList<FloorConfiguration> floors)
    {
        var hazardZoneValidator = new HazardZoneOptionsValidator();

        foreach (var floor in floors)
        {
            if (ReferenceEquals(floor.HazardZones, null))
            {
                return ValidateOptionsResult.Fail($"Floor '{floor.Name}': HazardZones configuration is missing.");
            }

            var hazardZoneOptions = new HazardZoneOptions { HazardZones = floor.HazardZones };
            var result = hazardZoneValidator.Validate(null, hazardZoneOptions);
            if (!result.Succeeded)
            {
                return ValidateOptionsResult.Fail($"Floor '{floor.Name}': {string.Join(", ", result.Failures ?? [])}");
            }
        }

        return ValidateOptionsResult.Success;
    }

    private static ValidateOptionsResult ValidateHazardZonesDoNotOverlap(IReadOnlyList<FloorConfiguration> floors)
    {
        foreach (var floor in floors)
        {
            if (ReferenceEquals(floor.HazardZones, null))
            {
                return ValidateOptionsResult.Fail($"Floor '{floor.Name}': HazardZones configuration is missing.");
            }

            var hazardZoneList = floor.HazardZones.ToList();
            var hazardZoneWithNullOutline = hazardZoneList.FirstOrDefault(hazardZone => ReferenceEquals(hazardZone.Outline, null));
            if (hazardZoneWithNullOutline is not null)
            {
                return ValidateOptionsResult.Fail($"HazardZone '{hazardZoneWithNullOutline.Name}' must have an outline.");
            }

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

        return ValidateOptionsResult.Success;
    }

    private static ValidateOptionsResult ValidateHazardZonesAreWithinFloorOutline(IReadOnlyList<FloorConfiguration> floors)
    {
        foreach (var floor in floors)
        {
            if (ReferenceEquals(floor.HazardZones, null))
            {
                return ValidateOptionsResult.Fail($"Floor '{floor.Name}': HazardZones configuration is missing.");
            }

            var floorOutline = ToOutline(floor.Outline);
            foreach (var hazardZone in floor.HazardZones)
            {
                if (ReferenceEquals(hazardZone.Outline, null))
                {
                    return ValidateOptionsResult.Fail($"HazardZone '{hazardZone.Name}' must have an outline.");
                }

                var hazardZoneOutline = ToOutline(hazardZone.Outline);
                if (!hazardZoneOutline.IsWithin(floorOutline))
                {
                    return ValidateOptionsResult.Fail($"HazardZone '{hazardZone.Name}' is not within floor '{floor.Name}'.");
                }
            }
        }

        return ValidateOptionsResult.Success;
    }

    private static Outline ToOutline(IReadOnlyList<Coordinate> points)
    {
        return new Outline(new ReadOnlyCollection<Coordinate>(points.ToList()));
    }
}
