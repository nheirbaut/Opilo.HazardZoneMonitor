using Opilo.HazardZoneMonitor.Api.Features.HazardZones.Configuration;
using Opilo.HazardZoneMonitor.Api.Shared.Cqrs;

namespace Opilo.HazardZoneMonitor.Api.Features.HazardZones.GetHazardZones;

public sealed record GetHazardZonesResponse(IReadOnlyList<HazardZoneInfo> HazardZones) : IResponse;
