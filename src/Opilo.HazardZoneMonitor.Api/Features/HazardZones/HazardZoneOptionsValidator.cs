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

        return ValidateOptionsResult.Success;
    }
}
