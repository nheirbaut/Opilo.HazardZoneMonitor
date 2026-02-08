namespace Opilo.HazardZoneMonitor.Api.Features.Floors.GetFloors;

public sealed record FloorItem(string Name, IReadOnlyList<FloorPointConfiguration> Outline);
