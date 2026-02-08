using Opilo.HazardZoneMonitor.Api.Shared.Cqrs;

namespace Opilo.HazardZoneMonitor.Api.Features.Floors.GetFloors;

public sealed record Response(IReadOnlyList<FloorConfiguration> Floors) : IResponse;
