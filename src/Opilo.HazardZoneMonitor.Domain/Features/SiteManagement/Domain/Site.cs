using Ardalis.GuardClauses;
using Opilo.HazardZoneMonitor.Domain.Features.FloorManagement.Domain;
using Opilo.HazardZoneMonitor.Domain.Shared.Guards;
using Opilo.HazardZoneMonitor.Domain.Shared.ValueObjects;

namespace Opilo.HazardZoneMonitor.Domain.Features.SiteManagement.Domain;

public sealed class Site
{
    public Site(SiteName name, IList<Floor> floors)
    {
        Guard.Against.Null(floors);

        var floorList = floors.ToList();
        Guard.Against.DuplicateFloor(floorList, nameof(floors));

        Name = name;
    }

    public SiteName Name { get; }
}
