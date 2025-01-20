using Ardalis.GuardClauses;

namespace Opilo.HazardZoneMonitor.Domain.Entities;

public sealed class HazardZone
{
    public string Name { get; }

    public HazardZone(string name)
    {
        Guard.Against.NullOrWhiteSpace(name);
        Name = name;
    }
}
