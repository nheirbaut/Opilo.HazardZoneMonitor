namespace Opilo.HazardZoneMonitor.Api.Features.PersonTracking;

public class RegisteredPersonMovementDto(Guid personId)
{
    public Guid PersonId { get; } = personId;
}
