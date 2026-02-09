using Ardalis.Result;

namespace Opilo.HazardZoneMonitor.Api.Shared.Cqrs;

public interface IQueryHandler<in TQuery, TResponse>
    where TQuery : IQuery<TResponse>
    where TResponse : IResponse
{
    Task<Result<TResponse>> Handle(TQuery query, CancellationToken cancellationToken);
}
