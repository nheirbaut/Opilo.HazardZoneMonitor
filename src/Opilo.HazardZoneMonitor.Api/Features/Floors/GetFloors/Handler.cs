using Microsoft.Extensions.Options;
using Opilo.HazardZoneMonitor.Api.Shared.Cqrs;

namespace Opilo.HazardZoneMonitor.Api.Features.Floors.GetFloors;

public sealed class Handler(IOptions<FloorOptions> floorOptions) : IQueryHandler<Query, Response>
{
    public Task<Response> Handle(Query query, CancellationToken cancellationToken)
    {
        return Task.FromResult(new Response(floorOptions.Value.Floors));
    }
}
