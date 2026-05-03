using Opilo.HazardZoneMonitor.Api.Shared.Configuration;
using Opilo.HazardZoneMonitor.Domain.Shared.Primitives;
using Opilo.HazardZoneMonitor.Domain.Shared.ValueObjects;

namespace Opilo.HazardZoneMonitor.Api.Features.HazardZones;

public sealed record HazardZoneConfiguration(
    HazardZoneName Name,
    IReadOnlyList<PointConfiguration> Outline,
    TimeSpan ActivationDuration,
    TimeSpan PreAlarmDuration,
    ZoneState ZoneState = default,
    AlarmState AlarmState = default,
    int AllowedNumberOfPersons = 0);
