using System.Diagnostics.CodeAnalysis;

namespace Opilo.HazardZoneMonitor.Domain.Shared.Primitives;

[SuppressMessage("Major Code Smell", "S4035:Classes implementing \"IEquatable<T>\" should be sealed",
    Justification = "ValueObject is intentionally an abstract base class for derived value objects; equality is implemented via GetEqualityComponents and includes a runtime type check.")]
public abstract class ValueObject : IEquatable<ValueObject>
{
    public static bool operator ==(ValueObject? left, ValueObject? right)
    {
        if (left is null)
        {
            return right is null;
        }

        return left.Equals(right);
    }

    public static bool operator !=(ValueObject? left, ValueObject? right) => !(left == right);

    public bool Equals(ValueObject? other)
    {
        if (other is null)
        {
            return false;
        }

        if (GetType() != other.GetType())
        {
            return false;
        }

        return GetEqualityComponents().SequenceEqual(other.GetEqualityComponents());
    }

    public override bool Equals(object? obj) => obj is ValueObject valueObject && Equals(valueObject);

    public override int GetHashCode() =>
        GetEqualityComponents()
            .Aggregate(17, (hash, component) => unchecked((hash * 23) + (component?.GetHashCode() ?? 0)));

    protected abstract IEnumerable<object?> GetEqualityComponents();
}
