using Ardalis.GuardClauses;
using Opilo.HazardZoneMonitor.Domain.ValueObjects;

namespace Opilo.HazardZoneMonitor.Domain.Entities;

public sealed class HazardZone
{
    public string Name { get; }
    public Outline Outline { get; }

    public HazardZone(string name, Outline outline)
    {
        Guard.Against.NullOrWhiteSpace(name);
        Guard.Against.Null(outline);

        Name = name;
        Outline = outline;
    }
}
