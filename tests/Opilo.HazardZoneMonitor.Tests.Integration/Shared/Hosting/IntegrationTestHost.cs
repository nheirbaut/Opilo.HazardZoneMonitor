using Microsoft.AspNetCore.Mvc.Testing;
using Opilo.HazardZoneMonitor.Api;

namespace Opilo.HazardZoneMonitor.Tests.Integration.Shared.Hosting;

public sealed class IntegrationTestHost(WebApplicationFactory<IApiMarker> factory) : IDisposable, IAsyncDisposable
{
    public IServiceProvider Services => factory.Services;

    public HttpClient CreateClient() => factory.CreateClient();

    public void Dispose() => factory.Dispose();

    public ValueTask DisposeAsync()
    {
        factory.Dispose();
        return ValueTask.CompletedTask;
    }
}
