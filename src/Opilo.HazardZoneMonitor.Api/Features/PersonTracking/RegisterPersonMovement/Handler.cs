using System.Collections.Concurrent;
using Opilo.HazardZoneMonitor.Api.Shared.Cqrs;

namespace Opilo.HazardZoneMonitor.Api.Features.PersonTracking.RegisterPersonMovement;

public sealed class Handler : ICommandHandler<Command, Response>
{
    internal static readonly ConcurrentDictionary<Guid, (Guid PersonId, double X, double Y, DateTime RegisteredAt)> Movements = new();

    public Task<Response> Handle(Command command, CancellationToken cancellationToken)
    {
        var id = Guid.CreateVersion7();
        Movements[id] = (command.PersonId, command.X, command.Y, DateTime.UtcNow);
        return Task.FromResult(new Response(id));
    }
}
