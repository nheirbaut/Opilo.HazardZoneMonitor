using Opilo.HazardZoneMonitor.Domain.Shared.Primitives;

namespace Opilo.HazardZoneMonitor.Api.Features.FloorManagement;

public sealed record FloorDto(string Id, string Name, Outline Outline);
