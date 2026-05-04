using Ardalis.GuardClauses;

namespace Opilo.HazardZoneMonitor.Domain.Shared.Primitives;

public sealed class SourceId : ValueObject
{
    private SourceId(string value)
    {
        Value = value;
    }

    public string Value { get; }

    public static SourceId From(string value)
    {
        Guard.Against.NullOrWhiteSpace(value);

        return new SourceId(value);
    }

    public override string ToString() => Value;

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }
}
