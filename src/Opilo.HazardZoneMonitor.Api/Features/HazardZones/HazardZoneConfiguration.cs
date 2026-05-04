using Opilo.HazardZoneMonitor.Domain.Shared.Primitives;

namespace Opilo.HazardZoneMonitor.Api.Features.HazardZones;

public sealed record HazardZoneConfiguration(
    string Name,
    IReadOnlyList<Coordinate> Outline,
    TimeSpan ActivationDuration,
    TimeSpan PreAlarmDuration,
    ZoneState ZoneState = default,
    AlarmState AlarmState = default,
    int AllowedNumberOfPersons = 0);
