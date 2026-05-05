using System.Net;
using Ardalis.Result;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using Opilo.HazardZoneMonitor.Api.Features.HazardZones.ActivateHazardZone;
using Opilo.HazardZoneMonitor.Api.Features.HazardZones.Configuration;
using Opilo.HazardZoneMonitor.Api.Shared.Cqrs;
using Opilo.HazardZoneMonitor.Domain.Shared.Primitives;
using Opilo.HazardZoneMonitor.Tests.Integration.Shared;

namespace Opilo.HazardZoneMonitor.Tests.Integration.Features.HazardZones;

public sealed class ActivateHazardZoneSpecification(CustomWebApplicationFactory factory)
    : IClassFixture<CustomWebApplicationFactory>
{
    [Fact]
    public async Task ActivateHazardZone_ShouldReturn404NotFound_WhenHazardZoneDoesNotExist()
    {
        // Arrange
        var mockHandler = Substitute.For<ICommandHandler<Command>>();
        mockHandler
            .Handle(Arg.Any<Command>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result.NotFound()));

        await using var customFactory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                services.AddScoped<ICommandHandler<Command>>(_ => mockHandler);
            });
        });

        var client = customFactory.CreateClient();
        using var emptyContent = new StringContent(string.Empty);

        // Act
        var response = await client.PostAsync(
            new Uri("/api/v1/hazard-zones/non-existing/activate", UriKind.Relative),
            emptyContent,
            TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task ActivateHazardZone_ShouldReturn204NoContent_WhenHazardZoneExists()
    {
        // Arrange
        var hazardZoneOptions = new HazardZoneOptions
        {
            HazardZones =
            [
                new HazardZoneConfiguration(
                    HazardZoneName.From("existing-hazardzone"),
                    [new Coordinate(0, 0), new Coordinate(10, 0), new Coordinate(10, 10), new Coordinate(0, 10)],
                    TimeSpan.Zero,
                    TimeSpan.Zero)
            ]
        };

        var mockHandler = Substitute.For<ICommandHandler<Command>>();
        mockHandler
            .Handle(Arg.Any<Command>(), Arg.Any<CancellationToken>())
            .Returns(ci =>
            {
                var cmd = ci.Arg<Command>();
                return Task.FromResult(
                    string.Equals(cmd.HazardZoneName.Value, "existing-hazardzone", StringComparison.Ordinal)
                        ? Result.NoContent()
                        : Result.NotFound());
            });

        await using var customFactory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureAppConfiguration((_, config) =>
            {
                config.AddInMemoryCollection(hazardZoneOptions.ToConfigurationDictionary());
            });
            builder.ConfigureServices(services =>
            {
                services.AddScoped<ICommandHandler<Command>>(_ => mockHandler);
            });
        });

        var client = customFactory.CreateClient();
        using var emptyContent = new StringContent(string.Empty);

        // Act
        var response = await client.PostAsync(
            new Uri("/api/v1/hazard-zones/existing-hazardzone/activate", UriKind.Relative),
            emptyContent,
            TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }
}
