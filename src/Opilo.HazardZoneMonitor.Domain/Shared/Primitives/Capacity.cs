using System.Globalization;
using Ardalis.GuardClauses;

namespace Opilo.HazardZoneMonitor.Domain.Shared.Primitives;

public sealed class Capacity : ValueObject
{
    private Capacity(int value)
    {
        Value = value;
    }

    public int Value { get; }

    public static Capacity From(int value)
    {
        Guard.Against.Negative(value);

        return new Capacity(value);
    }

    public override string ToString() => Value.ToString(CultureInfo.InvariantCulture);

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }
}
