using Opilo.HazardZoneMonitor.Api.Shared.Cqrs;

namespace Opilo.HazardZoneMonitor.Api.Features.PersonTracking.RegisterPersonMovement;

public class Response(Guid personId) : IResponse
{
    public Guid PersonId { get; } = personId;
}
