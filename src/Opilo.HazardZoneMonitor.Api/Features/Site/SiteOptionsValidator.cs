using Microsoft.Extensions.Options;

namespace Opilo.HazardZoneMonitor.Api.Features.Site;

public sealed class SiteOptionsValidator : IValidateOptions<SiteOptions>
{
    public ValidateOptionsResult Validate(string? name, SiteOptions options)
    {
        return ValidateOptionsResult.Success;
    }
}
