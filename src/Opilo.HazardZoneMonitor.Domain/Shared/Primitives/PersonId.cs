using Ardalis.GuardClauses;

namespace Opilo.HazardZoneMonitor.Domain.Shared.Primitives;

public sealed class PersonId : ValueObject
{
    private PersonId(Guid value)
    {
        Value = value;
    }

    public Guid Value { get; }

    public static PersonId From(Guid value)
    {
        Guard.Against.Default(value);

        return new PersonId(value);
    }

    public override string ToString() => Value.ToString();

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }
}
