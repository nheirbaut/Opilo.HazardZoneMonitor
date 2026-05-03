using Vogen;

namespace Opilo.HazardZoneMonitor.Domain.Shared.ValueObjects;

[ValueObject<string>]
public partial struct FloorName
{
    public static bool operator <(FloorName left, FloorName right) => left.CompareTo(right) < 0;
    public static bool operator >(FloorName left, FloorName right) => left.CompareTo(right) > 0;
    public static bool operator <=(FloorName left, FloorName right) => left.CompareTo(right) <= 0;
    public static bool operator >=(FloorName left, FloorName right) => left.CompareTo(right) >= 0;

    private static Validation Validate(string value) =>
        string.IsNullOrWhiteSpace(value) ? Validation.Invalid("FloorName cannot be empty.") : Validation.Ok;

    private static string NormalizeInput(string input) => input.ToUpperInvariant();
}
