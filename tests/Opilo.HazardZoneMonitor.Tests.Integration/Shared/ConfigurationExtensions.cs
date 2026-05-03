using System.Globalization;
using Opilo.HazardZoneMonitor.Api.Features.Floors;
using Opilo.HazardZoneMonitor.Api.Features.HazardZones;
using Opilo.HazardZoneMonitor.Api.Features.Site;
using Opilo.HazardZoneMonitor.Api.Shared.Configuration;

namespace Opilo.HazardZoneMonitor.Tests.Integration.Shared;

public static class ConfigurationExtensions
{
    public static IDictionary<string, string?> ToConfigurationDictionary(this FloorOptions floorOptions)
    {
        ArgumentNullException.ThrowIfNull(floorOptions);

        Dictionary<string, string?> dictionary = new(StringComparer.Ordinal);
        var floors = floorOptions.Floors;

        for (var floorIndex = 0; floorIndex < floors.Count; floorIndex++)
        {
            var floor = floors[floorIndex];
            var floorKey = $"{nameof(FloorOptions)}:{nameof(FloorOptions.Floors)}:{floorIndex}";

            dictionary[$"{floorKey}:{nameof(FloorConfiguration.Name)}"] = floor.Name.Value;

            for (var pointIndex = 0; pointIndex < floor.Outline.Count; pointIndex++)
            {
                var point = floor.Outline[pointIndex];
                var pointKey = $"{floorKey}:{nameof(FloorConfiguration.Outline)}:{pointIndex}";

                dictionary[$"{pointKey}:{nameof(PointConfiguration.X)}"] = point.X.ToString(CultureInfo.InvariantCulture);
                dictionary[$"{pointKey}:{nameof(PointConfiguration.Y)}"] = point.Y.ToString(CultureInfo.InvariantCulture);
            }

            if (floor.HazardZones is { Count: > 0 })
            {
                for (var hazardZoneIndex = 0; hazardZoneIndex < floor.HazardZones.Count; hazardZoneIndex++)
                {
                    var hazardZone = floor.HazardZones[hazardZoneIndex];
                    var hazardZoneKey = $"{floorKey}:{nameof(FloorConfiguration.HazardZones)}:{hazardZoneIndex}";

                    dictionary[$"{hazardZoneKey}:{nameof(HazardZoneConfiguration.Name)}"] = hazardZone.Name.Value;
                    dictionary[$"{hazardZoneKey}:{nameof(HazardZoneConfiguration.ActivationDuration)}"] = hazardZone.ActivationDuration.ToString("c", CultureInfo.InvariantCulture);
                    dictionary[$"{hazardZoneKey}:{nameof(HazardZoneConfiguration.PreAlarmDuration)}"] = hazardZone.PreAlarmDuration.ToString("c", CultureInfo.InvariantCulture);
                    dictionary[$"{hazardZoneKey}:{nameof(HazardZoneConfiguration.AllowedNumberOfPersons)}"] = hazardZone.AllowedNumberOfPersons.ToString(CultureInfo.InvariantCulture);

                    for (var pointIndex = 0; pointIndex < hazardZone.Outline.Count; pointIndex++)
                    {
                        var point = hazardZone.Outline[pointIndex];
                        var pointKey = $"{hazardZoneKey}:{nameof(HazardZoneConfiguration.Outline)}:{pointIndex}";

                        dictionary[$"{pointKey}:{nameof(PointConfiguration.X)}"] = point.X.ToString(CultureInfo.InvariantCulture);
                        dictionary[$"{pointKey}:{nameof(PointConfiguration.Y)}"] = point.Y.ToString(CultureInfo.InvariantCulture);
                    }
                }
            }
        }

        return dictionary;
    }

    public static IDictionary<string, string?> ToConfigurationDictionary(this SiteOptions siteOptions)
    {
        ArgumentNullException.ThrowIfNull(siteOptions);

        return new Dictionary<string, string?>(StringComparer.Ordinal)
        {
            [$"{nameof(SiteOptions)}:{nameof(SiteOptions.Name)}"] = siteOptions.Name.Value,
        };
    }

    public static IDictionary<string, string?> ToConfigurationDictionary(this HazardZoneOptions hazardZoneOptions)
    {
        ArgumentNullException.ThrowIfNull(hazardZoneOptions);

        Dictionary<string, string?> dictionary = new(StringComparer.Ordinal);
        var hazardZones = hazardZoneOptions.HazardZones;

        for (var hazardZoneIndex = 0; hazardZoneIndex < hazardZones.Count; hazardZoneIndex++)
        {
            var hazardZone = hazardZones[hazardZoneIndex];
            var hazardZoneKey = $"{nameof(HazardZoneOptions)}:{nameof(HazardZoneOptions.HazardZones)}:{hazardZoneIndex}";

            dictionary[$"{hazardZoneKey}:{nameof(HazardZoneConfiguration.Name)}"] = hazardZone.Name.Value;
            dictionary[$"{hazardZoneKey}:{nameof(HazardZoneConfiguration.ActivationDuration)}"] = hazardZone.ActivationDuration.ToString("c", CultureInfo.InvariantCulture);
            dictionary[$"{hazardZoneKey}:{nameof(HazardZoneConfiguration.PreAlarmDuration)}"] = hazardZone.PreAlarmDuration.ToString("c", CultureInfo.InvariantCulture);
            dictionary[$"{hazardZoneKey}:{nameof(HazardZoneConfiguration.AllowedNumberOfPersons)}"] = hazardZone.AllowedNumberOfPersons.ToString(CultureInfo.InvariantCulture);

            for (var pointIndex = 0; pointIndex < hazardZone.Outline.Count; pointIndex++)
            {
                var point = hazardZone.Outline[pointIndex];
                var pointKey = $"{hazardZoneKey}:{nameof(HazardZoneConfiguration.Outline)}:{pointIndex}";

                dictionary[$"{pointKey}:{nameof(PointConfiguration.X)}"] = point.X.ToString(CultureInfo.InvariantCulture);
                dictionary[$"{pointKey}:{nameof(PointConfiguration.Y)}"] = point.Y.ToString(CultureInfo.InvariantCulture);
            }
        }

        return dictionary;
    }
}
