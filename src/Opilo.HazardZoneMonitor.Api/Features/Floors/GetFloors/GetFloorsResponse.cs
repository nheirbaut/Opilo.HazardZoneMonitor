using Opilo.HazardZoneMonitor.Api.Features.Floors.Configuration;
using Opilo.HazardZoneMonitor.Api.Shared.Cqrs;

namespace Opilo.HazardZoneMonitor.Api.Features.Floors.GetFloors;

public sealed record GetFloorsResponse(IReadOnlyList<FloorConfiguration> Floors) : IResponse;
