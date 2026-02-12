namespace Opilo.HazardZoneMonitor.Api.Features.HazardZones.GetHazardZones;

public sealed record Response(IReadOnlyCollection<object> HazardZones);
