using System.Text.Json;
using System.Text.Json.Serialization;
using Opilo.HazardZoneMonitor.Api.Shared.Infrastructure;

namespace Opilo.HazardZoneMonitor.Tests.Integration.Shared;

internal static class SerializationOptions
{
    internal static JsonSerializerOptions Default { get; } = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter(), new PersonIdJsonConverter(), new FloorNameJsonConverter(), new HazardZoneNameJsonConverter() },
    };
}
