using Vogen;

namespace Opilo.HazardZoneMonitor.Domain.Shared.ValueObjects;

[ValueObject<string>]
public partial struct SiteName
{
    public static bool operator <(SiteName left, SiteName right) => left.CompareTo(right) < 0;
    public static bool operator >(SiteName left, SiteName right) => left.CompareTo(right) > 0;
    public static bool operator <=(SiteName left, SiteName right) => left.CompareTo(right) <= 0;
    public static bool operator >=(SiteName left, SiteName right) => left.CompareTo(right) >= 0;

    private static string NormalizeInput(string input) => input.ToUpperInvariant();
}
