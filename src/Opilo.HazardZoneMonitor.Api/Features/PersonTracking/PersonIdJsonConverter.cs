using System.Text.Json;
using System.Text.Json.Serialization;
using Opilo.HazardZoneMonitor.Domain.Shared.Primitives;

namespace Opilo.HazardZoneMonitor.Api.Features.PersonTracking;

internal sealed class PersonIdJsonConverter : JsonConverter<PersonId>
{
    public override PersonId Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var guid = reader.GetGuid();

        return PersonId.From(guid);
    }

    public override void Write(Utf8JsonWriter writer, PersonId value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.Value);
    }
}
