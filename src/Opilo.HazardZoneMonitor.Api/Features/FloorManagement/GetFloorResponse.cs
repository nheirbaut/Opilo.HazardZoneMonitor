namespace Opilo.HazardZoneMonitor.Api.Features.FloorManagement;

public sealed record GetFloorResponse(IReadOnlyList<FloorConfiguration> Floors);
