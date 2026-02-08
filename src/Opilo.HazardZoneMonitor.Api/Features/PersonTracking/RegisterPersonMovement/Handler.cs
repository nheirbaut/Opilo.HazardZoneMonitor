using Opilo.HazardZoneMonitor.Api.Shared.Cqrs;

namespace Opilo.HazardZoneMonitor.Api.Features.PersonTracking.RegisterPersonMovement;

public class Handler : ICommandHandler<Command, Response>
{
    public Task<Response> Handle(Command command, CancellationToken cancellationToken)
    {
        return Task.FromResult(new Response(Guid.NewGuid()));
    }
}
