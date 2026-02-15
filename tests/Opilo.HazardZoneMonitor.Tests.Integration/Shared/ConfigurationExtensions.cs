using System.Globalization;
using Opilo.HazardZoneMonitor.Api.Features.Floors;
using Opilo.HazardZoneMonitor.Api.Features.HazardZones;

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

            dictionary[$"{floorKey}:{nameof(FloorConfiguration.Name)}"] = floor.Name;

            for (var pointIndex = 0; pointIndex < floor.Outline.Count; pointIndex++)
            {
                var point = floor.Outline[pointIndex];
                var pointKey = $"{floorKey}:{nameof(FloorConfiguration.Outline)}:{pointIndex}";

                dictionary[$"{pointKey}:{nameof(FloorPointConfiguration.X)}"] = point.X.ToString(CultureInfo.InvariantCulture);
                dictionary[$"{pointKey}:{nameof(FloorPointConfiguration.Y)}"] = point.Y.ToString(CultureInfo.InvariantCulture);
            }
        }

        return dictionary;
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

            dictionary[$"{hazardZoneKey}:{nameof(HazardZoneConfiguration.Name)}"] = hazardZone.Name;

            for (var pointIndex = 0; pointIndex < hazardZone.Outline.Count; pointIndex++)
            {
                var point = hazardZone.Outline[pointIndex];
                var pointKey = $"{hazardZoneKey}:{nameof(HazardZoneConfiguration.Outline)}:{pointIndex}";

                dictionary[$"{pointKey}:{nameof(HazardZonePointConfiguration.X)}"] = point.X.ToString(CultureInfo.InvariantCulture);
                dictionary[$"{pointKey}:{nameof(HazardZonePointConfiguration.Y)}"] = point.Y.ToString(CultureInfo.InvariantCulture);
            }
        }

        return dictionary;
    }
}
