using System.Text.Json;
using System.Text.Json.Serialization;
using Opilo.HazardZoneMonitor.Domain.Shared.Primitives;

namespace Opilo.HazardZoneMonitor.Api.Features.HazardZones;

internal sealed class HazardZoneNameJsonConverter : JsonConverter<HazardZoneName>
{
    public override HazardZoneName Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var hazardZoneName = reader.GetString();

        if (hazardZoneName is null)
        {
            throw new JsonException("HazardZoneName value cannot be null.");
        }

        return HazardZoneName.From(hazardZoneName);
    }

    public override void Write(Utf8JsonWriter writer, HazardZoneName value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.Value);
    }
}
