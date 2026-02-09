using Opilo.HazardZoneMonitor.Api.Shared.Cqrs;

namespace Opilo.HazardZoneMonitor.Api.Features.PersonTracking.GetPersonMovements;

public sealed record Response(Guid Id, Guid PersonId, double X, double Y, DateTime RegisteredAt) : IResponse;
