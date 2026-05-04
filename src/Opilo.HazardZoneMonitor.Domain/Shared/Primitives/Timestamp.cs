namespace Opilo.HazardZoneMonitor.Domain.Shared.Primitives;

public sealed class Timestamp : ValueObject
{
    public Timestamp(DateTime value)
    {
        Value = value;
    }

    public DateTime Value { get; }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }
}
