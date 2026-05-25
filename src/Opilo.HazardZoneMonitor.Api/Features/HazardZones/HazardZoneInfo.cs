using Opilo.HazardZoneMonitor.Domain.Shared.Primitives;

namespace Opilo.HazardZoneMonitor.Api.Features.HazardZones;

public sealed record HazardZoneInfo(
    HazardZoneName Name,
    IReadOnlyList<Coordinate> Outline,
    TimeSpan ActivationDuration,
    TimeSpan PreAlarmDuration,
    int AllowedNumberOfPersons,
    ZoneState ZoneState,
    AlarmState AlarmState);
