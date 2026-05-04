using System.ComponentModel;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;
using Ardalis.GuardClauses;

namespace Opilo.HazardZoneMonitor.Domain.Shared.ValueObjects;

[JsonConverter(typeof(FloorNameJsonConverter))]
[TypeConverter(typeof(FloorNameTypeConverter))]
public readonly record struct FloorName(string Value) : IComparable<FloorName>, IFormattable
{
    public static bool operator <(FloorName left, FloorName right) => left.CompareTo(right) < 0;
    public static bool operator >(FloorName left, FloorName right) => left.CompareTo(right) > 0;
    public static bool operator <=(FloorName left, FloorName right) => left.CompareTo(right) <= 0;
    public static bool operator >=(FloorName left, FloorName right) => left.CompareTo(right) >= 0;

    public static FloorName From(string value)
    {
        Guard.Against.NullOrWhiteSpace(value);

        var normalized = value.ToUpperInvariant();

        return new FloorName(normalized);
    }

    public int CompareTo(FloorName other) => string.Compare(Value, other.Value, StringComparison.Ordinal);

    public string ToString(string? format, IFormatProvider? formatProvider) => Value;

    public override string ToString() => Value;

    private sealed class FloorNameTypeConverter : TypeConverter
    {
        public override bool CanConvertFrom(ITypeDescriptorContext? context, Type sourceType) => sourceType == typeof(string);

        public override bool CanConvertTo(ITypeDescriptorContext? context, Type? destinationType) => destinationType == typeof(string);

        public override object ConvertFrom(ITypeDescriptorContext? context, CultureInfo? culture, object value)
        {
            if (value is string stringValue)
            {
                return From(stringValue);
            }

            throw new NotSupportedException();
        }

        public override object ConvertTo(
            ITypeDescriptorContext? context,
            CultureInfo? culture,
            object? value,
            Type destinationType)
        {
            if (value is FloorName floorName && destinationType == typeof(string))
            {
                return floorName.Value;
            }

            throw new NotSupportedException();
        }
    }

    private sealed class FloorNameJsonConverter : JsonConverter<FloorName>
    {
        public override FloorName Read(
            ref Utf8JsonReader reader,
            Type typeToConvert,
            JsonSerializerOptions options)
        {
            if (reader.TokenType != JsonTokenType.String)
            {
                throw new JsonException();
            }

            var stringValue = reader.GetString();

            if (string.IsNullOrWhiteSpace(stringValue))
            {
                throw new JsonException();
            }

            return From(stringValue);
        }

        public override void Write(Utf8JsonWriter writer, FloorName value, JsonSerializerOptions options) =>
            writer.WriteStringValue(value.Value);
    }
}
