using Opilo.HazardZoneMonitor.Api.Shared.Cqrs;

namespace Opilo.HazardZoneMonitor.Api.Features.Floors.GetFloors;

public class GetFloorsHandler : IQueryHandler<GetFloorsQuery, GetFloorsResponse>
{
    public Task<GetFloorsResponse> Handle(GetFloorsQuery query, CancellationToken cancellationToken)
    {
        return Task.FromResult(new GetFloorsResponse([]));
    }
}
