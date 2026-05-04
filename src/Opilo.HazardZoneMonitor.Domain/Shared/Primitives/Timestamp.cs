namespace Opilo.HazardZoneMonitor.Domain.Shared.Primitives;

public sealed class Timestamp : ValueObject
{
    private Timestamp(DateTime value)
    {
        Value = value;
    }

    public DateTime Value { get; }

    public static Timestamp From(DateTime value)
    {
        return new Timestamp(value);
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }
}
