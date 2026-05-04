using Ardalis.GuardClauses;

namespace Opilo.HazardZoneMonitor.Domain.Shared.Primitives;

public sealed class Duration : ValueObject
{
    private Duration(TimeSpan value)
    {
        Value = value;
    }

    public TimeSpan Value { get; }

    public static Duration From(TimeSpan value)
    {
        Guard.Against.Negative(value);

        return new Duration(value);
    }

    public override string ToString() => Value.ToString();

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }
}
