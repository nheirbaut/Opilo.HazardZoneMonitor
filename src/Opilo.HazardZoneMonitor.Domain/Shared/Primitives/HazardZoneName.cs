using Ardalis.GuardClauses;
using System.ComponentModel;

namespace Opilo.HazardZoneMonitor.Domain.Shared.Primitives;

[TypeConverter(typeof(HazardZoneNameTypeConverter))]
public sealed class HazardZoneName : ValueObject
{
    private HazardZoneName(string value)
    {
        Value = value;
    }

    public string Value { get; }

    public static HazardZoneName From(string value)
    {
        Guard.Against.NullOrWhiteSpace(value);

        var normalized = value.ToUpperInvariant();

        return new HazardZoneName(normalized);
    }

    public override string ToString() => Value;

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }
}
