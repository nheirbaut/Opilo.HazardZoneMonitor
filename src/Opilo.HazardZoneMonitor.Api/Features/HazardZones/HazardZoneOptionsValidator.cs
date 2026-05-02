using Microsoft.Extensions.Options;

namespace Opilo.HazardZoneMonitor.Api.Features.HazardZones;

public sealed class HazardZoneOptionsValidator : IValidateOptions<HazardZoneOptions>
{
    public ValidateOptionsResult Validate(string? name, HazardZoneOptions options)
    {
        return ValidateOptionsResult.Success;
    }
}
