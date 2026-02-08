namespace Opilo.HazardZoneMonitor.Api.Shared.Cqrs;

public interface ICommandHandler<TCommand, TResponse>
    where TCommand : ICommand<TResponse>
    where TResponse : IResponse
{
    Task<TResponse> Handle(TCommand command, CancellationToken cancellationToken);
}
