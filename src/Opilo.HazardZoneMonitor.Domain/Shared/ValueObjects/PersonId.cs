using System.ComponentModel;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;
using Ardalis.GuardClauses;

namespace Opilo.HazardZoneMonitor.Domain.Shared.ValueObjects;

[JsonConverter(typeof(PersonIdJsonConverter))]
[TypeConverter(typeof(PersonIdTypeConverter))]
public readonly record struct PersonId(Guid Value) : IComparable<PersonId>, IFormattable
{
    public static bool operator <(PersonId left, PersonId right) => left.CompareTo(right) < 0;
    public static bool operator >(PersonId left, PersonId right) => left.CompareTo(right) > 0;
    public static bool operator <=(PersonId left, PersonId right) => left.CompareTo(right) <= 0;
    public static bool operator >=(PersonId left, PersonId right) => left.CompareTo(right) >= 0;

    public static PersonId From(Guid value)
    {
        Guard.Against.Default(value);

        return new PersonId(value);
    }

    public int CompareTo(PersonId other) => Value.CompareTo(other.Value);

    public string ToString(string? format, IFormatProvider? formatProvider) => Value.ToString(format, formatProvider);

    public override string ToString() => Value.ToString();

    private sealed class PersonIdTypeConverter : TypeConverter
    {
        public override bool CanConvertFrom(ITypeDescriptorContext? context, Type sourceType) =>
            sourceType == typeof(string) || sourceType == typeof(Guid);

        public override bool CanConvertTo(ITypeDescriptorContext? context, Type? destinationType) =>
            destinationType == typeof(string) || destinationType == typeof(Guid);

        public override object ConvertFrom(ITypeDescriptorContext? context, CultureInfo? culture, object value)
        {
            if (value is string stringValue)
            {
                return From(Guid.Parse(stringValue));
            }

            if (value is Guid guid)
            {
                return From(guid);
            }

            throw new NotSupportedException();
        }

        public override object ConvertTo(
            ITypeDescriptorContext? context,
            CultureInfo? culture,
            object? value,
            Type destinationType)
        {
            if (value is PersonId personId && destinationType == typeof(string))
            {
                return personId.Value.ToString();
            }

            if (value is PersonId personIdValue && destinationType == typeof(Guid))
            {
                return personIdValue.Value;
            }

            throw new NotSupportedException();
        }
    }

    private sealed class PersonIdJsonConverter : JsonConverter<PersonId>
    {
        public override PersonId Read(
            ref Utf8JsonReader reader,
            Type typeToConvert,
            JsonSerializerOptions options)
        {
            if (reader.TokenType != JsonTokenType.String)
            {
                throw new JsonException();
            }

            return From(reader.GetGuid());
        }

        public override void Write(Utf8JsonWriter writer, PersonId value, JsonSerializerOptions options) =>
            writer.WriteStringValue(value.Value.ToString());
    }
}
