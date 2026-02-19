using Ardalis.GuardClauses;
using Opilo.HazardZoneMonitor.Domain.Features.FloorManagement.Domain;
using Opilo.HazardZoneMonitor.Domain.Shared.Guards;

namespace Opilo.HazardZoneMonitor.Domain.Features.SiteManagement.Domain;

public sealed class Site
{
    public Site(string name, IList<Floor> floors)
    {
        Guard.Against.NullOrWhiteSpace(name);
        Guard.Against.Null(floors);

        var floorList = floors.ToList();
        Guard.Against.DuplicateFloor(floorList, nameof(floors));

        Name = name;
    }

    public string Name { get; }
}
