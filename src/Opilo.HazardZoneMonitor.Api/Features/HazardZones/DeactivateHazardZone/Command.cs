using Opilo.HazardZoneMonitor.Api.Shared.Cqrs;
using Opilo.HazardZoneMonitor.Domain.Shared.Primitives;

namespace Opilo.HazardZoneMonitor.Api.Features.HazardZones.DeactivateHazardZone;

public sealed record Command(HazardZoneName HazardZoneName) : ICommand;
