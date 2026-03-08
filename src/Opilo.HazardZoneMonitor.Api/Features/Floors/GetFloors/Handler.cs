using Ardalis.Result;
using Microsoft.Extensions.Options;
using Opilo.HazardZoneMonitor.Api.Shared.Cqrs;

namespace Opilo.HazardZoneMonitor.Api.Features.Floors.GetFloors;

public sealed class Handler(IOptions<FloorOptions> floorOptions) : IQueryHandler<Query, GetFloorsResponse>
{
    public Task<Result<GetFloorsResponse>> Handle(Query query, CancellationToken cancellationToken)
    {
        return Task.FromResult(Result.Success(new GetFloorsResponse(floorOptions.Value.Floors)));
    }
}
