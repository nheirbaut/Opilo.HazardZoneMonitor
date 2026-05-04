using Ardalis.GuardClauses;
using System.ComponentModel;

namespace Opilo.HazardZoneMonitor.Domain.Shared.Primitives;

[TypeConverter(typeof(FloorNameTypeConverter))]
public sealed class FloorName : ValueObject
{
    private FloorName(string value)
    {
        Value = value;
    }

    public string Value { get; }

    public static FloorName From(string value)
    {
        Guard.Against.NullOrWhiteSpace(value);

        var normalized = value.ToUpperInvariant();

        return new FloorName(normalized);
    }

    public override string ToString() => Value;

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }
}
