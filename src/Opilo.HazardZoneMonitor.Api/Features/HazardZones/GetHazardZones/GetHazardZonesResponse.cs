using Opilo.HazardZoneMonitor.Api.Shared.Cqrs;

namespace Opilo.HazardZoneMonitor.Api.Features.HazardZones.GetHazardZones;

public sealed record GetHazardZonesResponse(IReadOnlyList<HazardZoneConfiguration> HazardZones) : IResponse;
