using Microsoft.Extensions.Options;
using Opilo.HazardZoneMonitor.Api.Features.HazardZones;

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

        return ValidateOptionsResult.Success;
    }
}
