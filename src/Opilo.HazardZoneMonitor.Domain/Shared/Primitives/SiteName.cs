using Ardalis.GuardClauses;
using System.ComponentModel;

namespace Opilo.HazardZoneMonitor.Domain.Shared.Primitives;

[TypeConverter(typeof(SiteNameTypeConverter))]
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

        return new SiteName(value);
    }

    public override string ToString() => Value;

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }
}
