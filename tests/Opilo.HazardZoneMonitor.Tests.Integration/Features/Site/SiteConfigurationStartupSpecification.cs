using System.Net;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Opilo.HazardZoneMonitor.Api;
using Opilo.HazardZoneMonitor.Api.Features.Site;
using Opilo.HazardZoneMonitor.Tests.Integration.Shared;

namespace Opilo.HazardZoneMonitor.Tests.Integration.Features.Site;

public sealed class SiteConfigurationStartupSpecification(CustomWebApplicationFactory factory)
    : IClassFixture<CustomWebApplicationFactory>, IDisposable
{
    private WebApplicationFactory<IApiMarker>? _customFactory;

    [Fact]
    public async Task Api_ShouldStart_WhenSiteConfigurationIsValid()
    {
        // Arrange
        var client = factory.CreateClient();

        // Act
        var response = await client.GetAsync(
            new Uri("/api/v1/site", UriKind.Relative),
            TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public void Api_ShouldThrowOptionsValidationException_WhenSiteNameIsNull()
    {
        // Arrange
        var siteOptions = new SiteOptions { Name = null };

        _customFactory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureAppConfiguration((_, config) =>
            {
                config.AddInMemoryCollection(siteOptions.ToConfigurationDictionary());
            });
        });

        // Act
        Action act = () => _customFactory.CreateClient();

        // Assert
        act.Should().Throw<OptionsValidationException>();
    }

    [Fact]
    public void Api_ShouldThrowOptionsValidationException_WhenSiteNameIsEmpty()
    {
        // Arrange
        var siteOptions = new SiteOptions { Name = string.Empty };

        _customFactory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureAppConfiguration((_, config) =>
            {
                config.AddInMemoryCollection(siteOptions.ToConfigurationDictionary());
            });
        });

        // Act
        Action act = () => _customFactory.CreateClient();

        // Assert
        act.Should().Throw<OptionsValidationException>();
    }

    public void Dispose()
    {
        _customFactory?.Dispose();
    }
}
