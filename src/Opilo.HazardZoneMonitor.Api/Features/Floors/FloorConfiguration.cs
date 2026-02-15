using Opilo.HazardZoneMonitor.Api.Shared.Configuration;

namespace Opilo.HazardZoneMonitor.Api.Features.Floors;

public sealed record FloorConfiguration(string Name, IReadOnlyList<PointConfiguration> Outline);

