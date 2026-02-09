using System.Collections.Concurrent;
using Ardalis.Result;
using Opilo.HazardZoneMonitor.Api.Shared.Cqrs;

namespace Opilo.HazardZoneMonitor.Api.Features.PersonTracking.RegisterPersonMovement;

public sealed class Handler : ICommandHandler<Command, Response>
{
    internal static readonly ConcurrentDictionary<Guid, (Guid PersonId, double X, double Y, DateTime RegisteredAt)> Movements = new();

    public Task<Result<Response>> Handle(Command command, CancellationToken cancellationToken)
    {
        var registeredAt = DateTime.UtcNow;
        var response = new Response(command.PersonId, command.X, command.Y, registeredAt);
        Movements[response.Id] = (command.PersonId, command.X, command.Y, registeredAt);
        return Task.FromResult(Result.Created(response));
    }
}
