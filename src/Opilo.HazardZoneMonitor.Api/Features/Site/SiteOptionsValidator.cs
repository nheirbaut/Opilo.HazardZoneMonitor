using Microsoft.Extensions.Options;

namespace Opilo.HazardZoneMonitor.Api.Features.Site;

public sealed class SiteOptionsValidator : IValidateOptions<SiteOptions>
{
    public ValidateOptionsResult Validate(string? name, SiteOptions options)
        => string.IsNullOrWhiteSpace(options.Name)
            ? ValidateOptionsResult.Fail("SiteOptions:Name is required.")
            : ValidateOptionsResult.Success;
}
