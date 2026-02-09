using Ardalis.Result;

namespace Opilo.HazardZoneMonitor.Api.Shared.Cqrs;

public interface ICommandHandler<TCommand, TResponse>
    where TCommand : ICommand<TResponse>
    where TResponse : IResponse
{
    Task<Result<TResponse>> Handle(TCommand command, CancellationToken cancellationToken);
}
