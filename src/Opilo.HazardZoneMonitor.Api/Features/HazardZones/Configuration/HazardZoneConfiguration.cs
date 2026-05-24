using Opilo.HazardZoneMonitor.Domain.Shared.Primitives;

namespace Opilo.HazardZoneMonitor.Api.Features.HazardZones.Configuration;

public sealed record HazardZoneConfiguration(
    HazardZoneName Name,
    IReadOnlyList<Coordinate> Outline,
    TimeSpan ActivationDuration,
    TimeSpan PreAlarmDuration,
    int AllowedNumberOfPersons = 0);
