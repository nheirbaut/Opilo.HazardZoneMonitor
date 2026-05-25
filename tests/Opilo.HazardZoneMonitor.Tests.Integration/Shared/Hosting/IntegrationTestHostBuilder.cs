using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Opilo.HazardZoneMonitor.Api;
using Opilo.HazardZoneMonitor.Api.Features.Floors.Configuration;
using Opilo.HazardZoneMonitor.Api.Features.HazardZones.Configuration;
using Opilo.HazardZoneMonitor.Api.Features.Site.Configuration;
using Opilo.HazardZoneMonitor.Domain.Shared.Abstractions;
using Opilo.HazardZoneMonitor.Tests.Common.TestUtilities;

namespace Opilo.HazardZoneMonitor.Tests.Integration.Shared.Hosting;

public sealed class IntegrationTestHostBuilder
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly List<IDictionary<string, string?>> _configurationOverrides = [];
    private readonly List<Action<IServiceCollection>> _serviceOverrides = [];

    internal IntegrationTestHostBuilder(CustomWebApplicationFactory factory)
    {
        _factory = factory;
    }

    public IntegrationTestHostBuilder WithSiteConfiguration(SiteOptions siteOptions) =>
        WithConfiguration(siteOptions.ToConfigurationDictionary());

    public IntegrationTestHostBuilder WithFloorConfiguration(FloorOptions floorOptions) =>
        WithConfiguration(floorOptions.ToConfigurationDictionary());

    public IntegrationTestHostBuilder WithHazardZoneConfiguration(HazardZoneOptions hazardZoneOptions) =>
        WithConfiguration(hazardZoneOptions.ToConfigurationDictionary());

    public IntegrationTestHostBuilder WithSetting(string key, string? value) =>
        WithConfiguration(new Dictionary<string, string?>(StringComparer.Ordinal)
        {
            [key] = value,
        });

    public IntegrationTestHostBuilder WithDatabase(string databasePath) =>
        WithSetting("ConnectionStrings:DefaultConnection", $"Data Source={databasePath}");

    public IntegrationTestHostBuilder WithClock(IClock clock) =>
        WithServices(services =>
        {
            services.RemoveAll<IClock>();
            services.AddSingleton(clock);
        });

    public IntegrationTestHostBuilder WithTimerFactory(ITimerFactory timerFactory) =>
        WithServices(services =>
        {
            services.RemoveAll<ITimerFactory>();
            services.AddSingleton(timerFactory);
        });

    internal IntegrationTestHostBuilder WithFakeTime(FakeClock clock) =>
        WithClock(clock).WithTimerFactory(new FakeTimerFactory(clock));

    public IntegrationTestHostBuilder WithServices(Action<IServiceCollection> configureServices)
    {
        _serviceOverrides.Add(configureServices);
        return this;
    }

    public IntegrationTestHost Start()
    {
        WebApplicationFactory<IApiMarker> configuredFactory = _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureAppConfiguration((_, config) =>
            {
                foreach (var configurationOverride in _configurationOverrides)
                {
                    config.AddInMemoryCollection(configurationOverride);
                }
            });

            builder.ConfigureTestServices(services =>
            {
                foreach (var serviceOverride in _serviceOverrides)
                {
                    serviceOverride(services);
                }
            });
        });

        try
        {
            using var client = configuredFactory.CreateClient();
            return new IntegrationTestHost(configuredFactory);
        }
        catch
        {
            configuredFactory.Dispose();
            throw;
        }
    }

    private IntegrationTestHostBuilder WithConfiguration(IDictionary<string, string?> configuration)
    {
        _configurationOverrides.Add(configuration);
        return this;
    }
}
