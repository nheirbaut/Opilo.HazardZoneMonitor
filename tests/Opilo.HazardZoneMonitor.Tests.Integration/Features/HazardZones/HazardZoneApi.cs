using System.Net.Http.Json;
using System.Net;
using Opilo.HazardZoneMonitor.Api.Features.HazardZones;
using Opilo.HazardZoneMonitor.Api.Features.HazardZones.GetHazardZones;
using Opilo.HazardZoneMonitor.Domain.Shared.Primitives;
using Opilo.HazardZoneMonitor.Tests.Integration.Shared;

namespace Opilo.HazardZoneMonitor.Tests.Integration.Features.HazardZones;

internal static class HazardZoneApi
{
    internal static async Task ActivateHazardZone(HttpClient client, HazardZoneName hazardZoneName)
    {
        using var emptyContent = new StringContent(string.Empty);

        var response = await client.PostAsync(
            new Uri($"/api/v1/hazard-zones/{hazardZoneName.Value}/activate", UriKind.Relative),
            emptyContent,
            TestContext.Current.CancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var hazardZone = await GetCurrentHazardZone(client, hazardZoneName);
        hazardZone.ZoneState.Should().Be(ZoneState.Active);
    }

    internal static async Task<HazardZoneInfo> GetCurrentHazardZone(HttpClient client, HazardZoneName hazardZoneName)
    {
        var currentHazardZones = await client.GetFromJsonAsync<GetHazardZonesResponse>(
            new Uri("/api/v1/hazard-zones", UriKind.Relative),
            SerializationOptions.Default,
            TestContext.Current.CancellationToken);

        currentHazardZones.Should().NotBeNull();

        return currentHazardZones.HazardZones.Single(hazardZone => hazardZone.Name == hazardZoneName);
    }
}
