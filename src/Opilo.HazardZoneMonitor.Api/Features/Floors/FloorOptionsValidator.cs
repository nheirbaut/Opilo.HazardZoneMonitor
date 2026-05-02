using Microsoft.Extensions.Options;

namespace Opilo.HazardZoneMonitor.Api.Features.Floors;

public sealed class FloorOptionsValidator : IValidateOptions<FloorOptions>
{
    public ValidateOptionsResult Validate(string? name, FloorOptions options)
    {
        if (options.Floors.Any(f => string.IsNullOrWhiteSpace(f.Name)))
        {
            return ValidateOptionsResult.Fail("Each floor must have a non-empty name.");
        }

        if (options.Floors.Select(f => f.Name).Distinct(StringComparer.OrdinalIgnoreCase).Count() != options.Floors.Count)
        {
            return ValidateOptionsResult.Fail("Floor names must be unique (case-insensitive).");
        }
        return ValidateOptionsResult.Success;
    }
}
