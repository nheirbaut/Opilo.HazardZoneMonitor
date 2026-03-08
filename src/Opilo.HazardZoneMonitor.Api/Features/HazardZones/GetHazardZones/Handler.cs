using Ardalis.Result;
using Microsoft.Extensions.Options;
using Opilo.HazardZoneMonitor.Api.Shared.Cqrs;

namespace Opilo.HazardZoneMonitor.Api.Features.HazardZones.GetHazardZones;

public sealed class Handler(IOptions<HazardZoneOptions> hazardZoneOptions) : IQueryHandler<Query, GetHazardZonesResponse>
{
    public Task<Result<GetHazardZonesResponse>> Handle(Query query, CancellationToken cancellationToken)
    {
        return Task.FromResult(Result.Success(new GetHazardZonesResponse(hazardZoneOptions.Value.HazardZones)));
    }
}
