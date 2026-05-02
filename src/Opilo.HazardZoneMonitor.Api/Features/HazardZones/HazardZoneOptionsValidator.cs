using Microsoft.Extensions.Options;

namespace Opilo.HazardZoneMonitor.Api.Features.HazardZones;

public sealed class HazardZoneOptionsValidator : IValidateOptions<HazardZoneOptions>
{
    public ValidateOptionsResult Validate(string? name, HazardZoneOptions options)
    {
        if (options.HazardZones.Any(hazardZone => string.IsNullOrWhiteSpace(hazardZone.Name)))
        {
            return ValidateOptionsResult.Fail("Each hazard zone must have a non-empty name.");
        }

        if (options.HazardZones.Select(hazardZone => hazardZone.Name).Distinct(StringComparer.OrdinalIgnoreCase).Count() != options.HazardZones.Count)
        {
            return ValidateOptionsResult.Fail("Hazard zone names must be unique (case-insensitive).");
        }

        if (options.HazardZones.Any(hazardZone => hazardZone.Outline.Count < 3))
        {
            return ValidateOptionsResult.Fail("Each hazard zone's outline must have at least 3 points.");
        }

        if (options.HazardZones.Any(hazardZone => hazardZone.ActivationDuration < TimeSpan.Zero))
        {
            return ValidateOptionsResult.Fail("Each hazard zone's activation duration must not be negative.");
        }

        if (options.HazardZones.Any(hazardZone => hazardZone.PreAlarmDuration < TimeSpan.Zero))
        {
            return ValidateOptionsResult.Fail("Each hazard zone's pre-alarm duration must not be negative.");
        }

        if (options.HazardZones.Any(hazardZone => hazardZone.AllowedNumberOfPersons < 0))
        {
            return ValidateOptionsResult.Fail("Each hazard zone's allowed number of persons must not be negative.");
        }

        return ValidateOptionsResult.Success;
    }
}
