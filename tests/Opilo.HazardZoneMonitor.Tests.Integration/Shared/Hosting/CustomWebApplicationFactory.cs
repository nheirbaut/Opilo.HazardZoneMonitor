using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Opilo.HazardZoneMonitor.Api;

namespace Opilo.HazardZoneMonitor.Tests.Integration.Shared.Hosting;

public sealed class CustomWebApplicationFactory : WebApplicationFactory<IApiMarker>
{
    private readonly string _databasePath;
    private readonly bool _ownsDatabase;

    internal CustomWebApplicationFactory(string databasePath)
        : this(databasePath, ownsDatabase: false)
    {
    }

    private CustomWebApplicationFactory(string databasePath, bool ownsDatabase)
    {
        _databasePath = databasePath;
        _ownsDatabase = ownsDatabase;
    }

    public IntegrationTestHostBuilder CreateHost() => new(this);

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.UseEnvironment("Development");

        builder.ConfigureAppConfiguration((_, config) =>
        {
            config.AddInMemoryCollection(TestHostDefaults.CreateSettings(_databasePath));
        });
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);

        if (_ownsDatabase && File.Exists(_databasePath))
        {
            File.Delete(_databasePath);
        }
    }
}
