using Ardalis.GuardClauses;
using Opilo.HazardZoneMonitor.Domain.Features.FloorManagement.Domain;

namespace Opilo.HazardZoneMonitor.Domain.Shared.Guards;

public static class FloorGuards
{
    public static void DuplicateFloor(
        this IGuardClause guardClause,
        IReadOnlyCollection<Floor> floors,
        string parameterName)
    {
        ArgumentNullException.ThrowIfNull(floors);

        var names = floors.Select(f => f.Name).ToList();
        var distinctNames = names.Distinct().Count();
        if (distinctNames != names.Count)
        {
            throw new ArgumentException("Duplicate Floor names are not allowed.", parameterName);
        }
    }
}
