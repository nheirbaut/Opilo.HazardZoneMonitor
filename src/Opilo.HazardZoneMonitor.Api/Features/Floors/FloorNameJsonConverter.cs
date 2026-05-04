using System.Text.Json;
using System.Text.Json.Serialization;
using Opilo.HazardZoneMonitor.Domain.Shared.Primitives;

namespace Opilo.HazardZoneMonitor.Api.Features.Floors;

internal sealed class FloorNameJsonConverter : JsonConverter<FloorName>
{
    public override FloorName Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var floorName = reader.GetString();

        return FloorName.From(floorName!);
    }

    public override void Write(Utf8JsonWriter writer, FloorName value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.Value);
    }
}
