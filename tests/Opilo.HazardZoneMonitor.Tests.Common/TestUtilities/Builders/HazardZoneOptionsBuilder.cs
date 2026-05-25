using Opilo.HazardZoneMonitor.Api.Features.HazardZones.Configuration;

namespace Opilo.HazardZoneMonitor.Tests.Common.TestUtilities.Builders;

internal sealed class HazardZoneOptionsBuilder
{
    private IReadOnlyList<HazardZoneConfiguration> _hazardZones = [];

    public static HazardZoneOptionsBuilder Create() => new();

    public HazardZoneOptionsBuilder WithHazardZone(string name, Action<HazardZoneConfigurationBuilder>? configure = null)
    {
        var builder = HazardZoneConfigurationBuilder.Create().WithName(name);
        configure?.Invoke(builder);

        _hazardZones = [.. _hazardZones, builder.Build()];
        return this;
    }

    public HazardZoneOptionsBuilder WithHazardZone(HazardZoneConfiguration hazardZone)
    {
        _hazardZones = [.. _hazardZones, hazardZone];
        return this;
    }

    public HazardZoneOptionsBuilder WithHazardZones(params HazardZoneConfiguration[] hazardZones)
    {
        _hazardZones = [.. _hazardZones, .. hazardZones];
        return this;
    }

    public HazardZoneOptions Build() => new()
    {
        HazardZones = _hazardZones,
    };
}
