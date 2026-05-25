using Ardalis.Result;
using Opilo.HazardZoneMonitor.Api.Features.HazardZones.Services;
using Opilo.HazardZoneMonitor.Api.Shared.Cqrs;

namespace Opilo.HazardZoneMonitor.Api.Features.HazardZones.GetHazardZones;

public sealed class Handler(IHazardZoneService hazardZoneService) : IQueryHandler<Query, GetHazardZonesResponse>
{
    public Task<Result<GetHazardZonesResponse>> Handle(Query query, CancellationToken cancellationToken)
    {
        return Task.FromResult(Result.Success(new GetHazardZonesResponse(hazardZoneService.GetHazardZones())));
    }
}
