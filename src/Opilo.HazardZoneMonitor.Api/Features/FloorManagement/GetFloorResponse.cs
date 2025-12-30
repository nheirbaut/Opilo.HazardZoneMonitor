namespace Opilo.HazardZoneMonitor.Api.Features.FloorManagement;

[System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1515:Consider making public types internal", Justification = "Required for integration tests")]
public sealed record GetFloorResponse(IReadOnlyList<FloorDto> Floors);

