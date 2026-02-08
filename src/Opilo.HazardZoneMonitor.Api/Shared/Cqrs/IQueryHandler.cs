namespace Opilo.HazardZoneMonitor.Api.Shared.Cqrs;

public interface IQueryHandler<in TQuery, TResult>
{
    Task<TResult> Handle(TQuery query, CancellationToken cancellationToken);
}
