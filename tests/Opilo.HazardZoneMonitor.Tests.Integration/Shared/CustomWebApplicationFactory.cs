using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Opilo.HazardZoneMonitor.Api;

namespace Opilo.HazardZoneMonitor.Tests.Integration.Shared;

public sealed class CustomWebApplicationFactory : WebApplicationFactory<IApiMarker>
{
    private readonly string _databasePath;

    public CustomWebApplicationFactory()
        : this(Path.Combine(Path.GetTempPath(), $"hazardzone_test_{Guid.NewGuid():N}.db"))
    {
    }

    internal CustomWebApplicationFactory(string databasePath)
    {
        _databasePath = databasePath;
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.UseEnvironment("Development");

        builder.ConfigureAppConfiguration((_, config) =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>(StringComparer.Ordinal)
            {
                ["ConnectionStrings:DefaultConnection"] = $"Data Source={_databasePath}",
            });
        });
    }
}
