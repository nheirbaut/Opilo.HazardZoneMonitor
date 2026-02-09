using Opilo.HazardZoneMonitor.Api.Shared.Cqrs;

namespace Opilo.HazardZoneMonitor.Api.Features.PersonTracking.RegisterPersonMovement;

public sealed class Response(Guid personId, double x, double y) : IResponse
{
    public Guid Id { get; } = Guid.CreateVersion7();
    public Guid PersonId => personId;
    public double X => x;
    public double Y => y;
}
