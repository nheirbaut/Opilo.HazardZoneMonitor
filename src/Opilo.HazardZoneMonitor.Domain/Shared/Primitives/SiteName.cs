using Ardalis.GuardClauses;

namespace Opilo.HazardZoneMonitor.Domain.Shared.Primitives;

public sealed class SiteName : ValueObject
{
    private SiteName(string value)
    {
        Value = value;
    }

    public string Value { get; }

    public static SiteName From(string value)
    {
        Guard.Against.NullOrWhiteSpace(value);

        var normalized = value.ToUpperInvariant();

        return new SiteName(normalized);
    }

    public override string ToString() => Value;

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }
}
