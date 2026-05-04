using Microsoft.Extensions.Options;

namespace Opilo.HazardZoneMonitor.Api.Features.HazardZones;

public sealed class HazardZoneOptionsValidator : IValidateOptions<HazardZoneOptions>
{
    public ValidateOptionsResult Validate(string? name, HazardZoneOptions options)
    {
        if (ReferenceEquals(options.HazardZones, null))
        {
            return ValidateOptionsResult.Fail("HazardZones configuration is missing.");
        }

        var result = ValidateHazardZoneNamesAreUnique(options.HazardZones);
        if (!result.Succeeded) return result;

        result = ValidateHazardZoneOutlinesHaveMinimumPoints(options.HazardZones);
        if (!result.Succeeded) return result;

        result = ValidateHazardZoneActivationDurationsAreNotNegative(options.HazardZones);
        if (!result.Succeeded) return result;

        result = ValidateHazardZonePreAlarmDurationsAreNotNegative(options.HazardZones);
        if (!result.Succeeded) return result;

        result = ValidateHazardZoneAllowedNumberOfPersonsAreNotNegative(options.HazardZones);
        if (!result.Succeeded) return result;

        return ValidateOptionsResult.Success;
    }

    private static ValidateOptionsResult ValidateHazardZoneNamesAreUnique(IReadOnlyList<HazardZoneConfiguration> hazardZones)
    {
        var distinctCount = hazardZones.Select(hazardZone => hazardZone.Name).Distinct(StringComparer.OrdinalIgnoreCase).Count();
        if (distinctCount != hazardZones.Count)
        {
            return ValidateOptionsResult.Fail("Hazard zone names must be unique (case-insensitive).");
        }

        return ValidateOptionsResult.Success;
    }

    private static ValidateOptionsResult ValidateHazardZoneOutlinesHaveMinimumPoints(IReadOnlyList<HazardZoneConfiguration> hazardZones)
    {
        var invalidHazardZone = hazardZones.FirstOrDefault(HasInvalidOutline);
        if (invalidHazardZone is not null)
        {
            if (ReferenceEquals(invalidHazardZone.Outline, null))
            {
                return ValidateOptionsResult.Fail("Each hazard zone must have an outline.");
            }

            return ValidateOptionsResult.Fail("Each hazard zone's outline must have at least 3 points.");
        }

        return ValidateOptionsResult.Success;
    }

    private static bool HasInvalidOutline(HazardZoneConfiguration hazardZone)
    {
        if (ReferenceEquals(hazardZone.Outline, null))
        {
            return true;
        }

        return hazardZone.Outline.Count < 3;
    }

    private static ValidateOptionsResult ValidateHazardZoneActivationDurationsAreNotNegative(IReadOnlyList<HazardZoneConfiguration> hazardZones)
    {
        if (hazardZones.Any(hazardZone => hazardZone.ActivationDuration < TimeSpan.Zero))
        {
            return ValidateOptionsResult.Fail("Each hazard zone's activation duration must not be negative.");
        }

        return ValidateOptionsResult.Success;
    }

    private static ValidateOptionsResult ValidateHazardZonePreAlarmDurationsAreNotNegative(IReadOnlyList<HazardZoneConfiguration> hazardZones)
    {
        if (hazardZones.Any(hazardZone => hazardZone.PreAlarmDuration < TimeSpan.Zero))
        {
            return ValidateOptionsResult.Fail("Each hazard zone's pre-alarm duration must not be negative.");
        }

        return ValidateOptionsResult.Success;
    }

    private static ValidateOptionsResult ValidateHazardZoneAllowedNumberOfPersonsAreNotNegative(IReadOnlyList<HazardZoneConfiguration> hazardZones)
    {
        if (hazardZones.Any(hazardZone => hazardZone.AllowedNumberOfPersons < 0))
        {
            return ValidateOptionsResult.Fail("Each hazard zone's allowed number of persons must not be negative.");
        }

        return ValidateOptionsResult.Success;
    }
}
