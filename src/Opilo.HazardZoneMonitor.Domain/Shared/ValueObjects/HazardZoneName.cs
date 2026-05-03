using Vogen;

namespace Opilo.HazardZoneMonitor.Domain.Shared.ValueObjects;

[ValueObject<string>]
public partial struct HazardZoneName
{
    public static bool operator <(HazardZoneName left, HazardZoneName right) => left.CompareTo(right) < 0;
    public static bool operator >(HazardZoneName left, HazardZoneName right) => left.CompareTo(right) > 0;
    public static bool operator <=(HazardZoneName left, HazardZoneName right) => left.CompareTo(right) <= 0;
    public static bool operator >=(HazardZoneName left, HazardZoneName right) => left.CompareTo(right) >= 0;

    private static Validation Validate(string value) =>
        string.IsNullOrWhiteSpace(value) ? Validation.Invalid("HazardZoneName cannot be empty.") : Validation.Ok;

    private static string NormalizeInput(string input) => input.ToUpperInvariant();
}
