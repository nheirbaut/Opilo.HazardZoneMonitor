namespace Opilo.HazardZoneMonitor.Api.Features.Floors;

public sealed record FloorConfiguration(string Name, IReadOnlyList<FloorPointConfiguration> Outline);

