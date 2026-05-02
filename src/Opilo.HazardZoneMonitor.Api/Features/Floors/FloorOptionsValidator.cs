using Microsoft.Extensions.Options;

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

        if (options.Floors.SelectMany(floor => floor.HazardZones).Any(hazardZone => string.IsNullOrWhiteSpace(hazardZone.Name)))
        {
            return ValidateOptionsResult.Fail("Each hazard zone must have a non-empty name.");
        }

        foreach (var floor in options.Floors)
        {
            if (floor.HazardZones.Select(hazardZone => hazardZone.Name).Distinct(StringComparer.OrdinalIgnoreCase).Count() != floor.HazardZones.Count)
            {
                return ValidateOptionsResult.Fail($"Hazard zone names must be unique within floor '{floor.Name}'.");
            }
        }

        if (options.Floors.SelectMany(floor => floor.HazardZones).Any(hazardZone => hazardZone.Outline.Count < 3))
        {
            return ValidateOptionsResult.Fail("Each hazard zone's outline must have at least 3 points.");
        }

        if (options.Floors.SelectMany(floor => floor.HazardZones).Any(hazardZone => hazardZone.ActivationDuration < TimeSpan.Zero))
        {
            return ValidateOptionsResult.Fail("Each hazard zone's activation duration must not be negative.");
        }

        if (options.Floors.SelectMany(floor => floor.HazardZones).Any(hazardZone => hazardZone.PreAlarmDuration < TimeSpan.Zero))
        {
            return ValidateOptionsResult.Fail("Each hazard zone's pre-alarm duration must not be negative.");
        }

        return ValidateOptionsResult.Success;
    }
}
