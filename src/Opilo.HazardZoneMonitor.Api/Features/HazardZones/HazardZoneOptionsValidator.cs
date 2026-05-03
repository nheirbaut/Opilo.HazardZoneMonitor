using Microsoft.Extensions.Options;

namespace Opilo.HazardZoneMonitor.Api.Features.HazardZones;

public sealed class HazardZoneOptionsValidator : IValidateOptions<HazardZoneOptions>
{
    public ValidateOptionsResult Validate(string? name, HazardZoneOptions options)
    {
        var result = ValidateHazardZoneNamesAreNotEmpty(options);
        if (!result.Succeeded) return result;

        result = ValidateHazardZoneNamesAreUnique(options);
        if (!result.Succeeded) return result;

        result = ValidateHazardZoneOutlinesHaveMinimumPoints(options);
        if (!result.Succeeded) return result;

        result = ValidateHazardZoneActivationDurationsAreNotNegative(options);
        if (!result.Succeeded) return result;

        result = ValidateHazardZonePreAlarmDurationsAreNotNegative(options);
        if (!result.Succeeded) return result;

        result = ValidateHazardZoneAllowedNumberOfPersonsAreNotNegative(options);
        if (!result.Succeeded) return result;

        return ValidateOptionsResult.Success;
    }

    private static ValidateOptionsResult ValidateHazardZoneNamesAreNotEmpty(HazardZoneOptions options)
    {
        if (options.HazardZones.Any(hazardZone => string.IsNullOrWhiteSpace(hazardZone.Name)))
        {
            return ValidateOptionsResult.Fail("Each hazard zone must have a non-empty name.");
        }

        return ValidateOptionsResult.Success;
    }

    private static ValidateOptionsResult ValidateHazardZoneNamesAreUnique(HazardZoneOptions options)
    {
        if (options.HazardZones.Select(hazardZone => hazardZone.Name).Distinct(StringComparer.OrdinalIgnoreCase).Count() != options.HazardZones.Count)
        {
            return ValidateOptionsResult.Fail("Hazard zone names must be unique (case-insensitive).");
        }

        return ValidateOptionsResult.Success;
    }

    private static ValidateOptionsResult ValidateHazardZoneOutlinesHaveMinimumPoints(HazardZoneOptions options)
    {
        if (options.HazardZones.Any(hazardZone => hazardZone.Outline.Count < 3))
        {
            return ValidateOptionsResult.Fail("Each hazard zone's outline must have at least 3 points.");
        }

        return ValidateOptionsResult.Success;
    }

    private static ValidateOptionsResult ValidateHazardZoneActivationDurationsAreNotNegative(HazardZoneOptions options)
    {
        if (options.HazardZones.Any(hazardZone => hazardZone.ActivationDuration < TimeSpan.Zero))
        {
            return ValidateOptionsResult.Fail("Each hazard zone's activation duration must not be negative.");
        }

        return ValidateOptionsResult.Success;
    }

    private static ValidateOptionsResult ValidateHazardZonePreAlarmDurationsAreNotNegative(HazardZoneOptions options)
    {
        if (options.HazardZones.Any(hazardZone => hazardZone.PreAlarmDuration < TimeSpan.Zero))
        {
            return ValidateOptionsResult.Fail("Each hazard zone's pre-alarm duration must not be negative.");
        }

        return ValidateOptionsResult.Success;
    }

    private static ValidateOptionsResult ValidateHazardZoneAllowedNumberOfPersonsAreNotNegative(HazardZoneOptions options)
    {
        if (options.HazardZones.Any(hazardZone => hazardZone.AllowedNumberOfPersons < 0))
        {
            return ValidateOptionsResult.Fail("Each hazard zone's allowed number of persons must not be negative.");
        }

        return ValidateOptionsResult.Success;
    }
}
