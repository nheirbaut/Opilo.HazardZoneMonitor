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

        return new FloorName(value);
    }

    public override string ToString() => Value;

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }
}
