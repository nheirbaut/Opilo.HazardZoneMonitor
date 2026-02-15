using Opilo.HazardZoneMonitor.Api.Shared.Configuration;
using Opilo.HazardZoneMonitor.Domain.Shared.Primitives;

namespace Opilo.HazardZoneMonitor.Api.Features.HazardZones;

public sealed record HazardZoneConfiguration(
    string Name,
    IReadOnlyList<PointConfiguration> Outline,
    TimeSpan ActivationDuration = default,
    TimeSpan PreAlarmDuration = default,
    ZoneState ZoneState = default,
    AlarmState AlarmState = default,
    int AllowedNumberOfPersons = 0);
