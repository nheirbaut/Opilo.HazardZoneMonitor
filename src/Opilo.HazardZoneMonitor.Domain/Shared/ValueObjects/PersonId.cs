using Vogen;

namespace Opilo.HazardZoneMonitor.Domain.Shared.ValueObjects;

[ValueObject<Guid>]
public partial struct PersonId
{
    public static bool operator <(PersonId left, PersonId right) => left.CompareTo(right) < 0;
    public static bool operator >(PersonId left, PersonId right) => left.CompareTo(right) > 0;
    public static bool operator <=(PersonId left, PersonId right) => left.CompareTo(right) <= 0;
    public static bool operator >=(PersonId left, PersonId right) => left.CompareTo(right) >= 0;
}
