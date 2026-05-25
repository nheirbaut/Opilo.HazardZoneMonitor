using Opilo.HazardZoneMonitor.Api.Features.Site.Configuration;
using Opilo.HazardZoneMonitor.Domain.Shared.Primitives;

namespace Opilo.HazardZoneMonitor.Tests.Common.TestUtilities.Builders;

internal sealed class SiteOptionsBuilder
{
    private SiteName _name = SiteName.From("Test Site");

    public static SiteOptionsBuilder Create() => new();

    public SiteOptionsBuilder WithName(string name)
    {
        _name = SiteName.From(name);
        return this;
    }

    public SiteOptions Build() => new()
    {
        Name = _name,
    };
}
