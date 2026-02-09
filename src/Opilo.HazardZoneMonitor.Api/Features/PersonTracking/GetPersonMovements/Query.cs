using Opilo.HazardZoneMonitor.Api.Shared.Cqrs;

namespace Opilo.HazardZoneMonitor.Api.Features.PersonTracking.GetPersonMovements;

public sealed record Query(Guid Id) : IQuery<Response>;
