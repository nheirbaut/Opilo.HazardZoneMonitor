using System.Globalization;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Opilo.HazardZoneMonitor.Api.Features.Floors;

namespace Opilo.HazardZoneMonitor.Tests.Integration.Shared;

public static class ConfigurationExtensions
{
    public static Dictionary<string, string?> ToConfigurationDictionary<T>(
        this IEnumerable<T> items,
        string sectionName)
    {
        var json = JsonSerializer.Serialize(new Dictionary<string, IEnumerable<T>>
        {
            [sectionName] = items
        });

        var configuration = new ConfigurationBuilder()
            .AddJsonStream(new MemoryStream(System.Text.Encoding.UTF8.GetBytes(json)))
            .Build();

        return configuration.AsEnumerable()
            .Where(kvp => kvp.Value != null)
            .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
    }

    public static Dictionary<string, string?> ToConfigurationDictionary(this FloorOptions floorOptions)
    {
        ArgumentNullException.ThrowIfNull(floorOptions);

        var dictionary = new Dictionary<string, string?>();
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
}
