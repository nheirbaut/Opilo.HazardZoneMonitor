using System.Net;
using System.Net.Http.Json;
using Microsoft.Extensions.Configuration;
using Opilo.HazardZoneMonitor.Api.Features.Floors;
using Opilo.HazardZoneMonitor.Api.Features.Floors.GetFloors;
using Opilo.HazardZoneMonitor.Api.Shared.Configuration;
using Opilo.HazardZoneMonitor.Tests.Integration.Shared;

namespace Opilo.HazardZoneMonitor.Tests.Integration.Features.Floors;

public sealed class GetFloorsSpecification(CustomWebApplicationFactory factory)
    : IClassFixture<CustomWebApplicationFactory>
{
    [Fact]
    public async Task GetFloors_ShouldReturn200Ok_WhenCalled()
    {
        // Arrange
        var client = factory.CreateClient();

        // Act
        var response = await client.GetAsync(new Uri("/api/v1/floors", UriKind.Relative), TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetFloors_ShouldSendResponseWithoutFloors_WhenNoFloorsAreRegistered()
    {
        // Arrange
        var client = factory.CreateClient();

        // Act
        var response = await client.GetFromJsonAsync<Response>(new Uri("/api/v1/floors", UriKind.Relative), TestContext.Current.CancellationToken);

        // Assert
        response.Should().NotBeNull();
        response.Floors.Should().NotBeNull();
        response.Floors.Should().BeEmpty();
    }

    [Fact]
    public async Task GetFloors_ShouldSendResponseWithFloors_WhenFloorsAreRegistered()
    {
        // Arrange
        List<FloorConfiguration> expectedFloors =
        [
            new("First Floor",
                new List<PointConfiguration>
                {
                    new(0, 0),
                    new(10, 0),
                    new(10, 10),
                    new(0, 10)
                }),
            new("Second Floor",
                new List<PointConfiguration>
                {
                    new(0, 0),
                    new(15, 0),
                    new(15, 15),
                    new(0, 15)
                })
        ];
        var floorOptions = new FloorOptions { Floors = expectedFloors };

        await using var customFactory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureAppConfiguration((_, config) =>
            {
                config.AddInMemoryCollection(floorOptions.ToConfigurationDictionary());
            });
        });

        var client = customFactory.CreateClient();

        // Act
        var response = await client.GetFromJsonAsync<Response>(new Uri("/api/v1/floors", UriKind.Relative), TestContext.Current.CancellationToken);

        // Assert
        response.Should().NotBeNull();
        response.Floors.Should().NotBeNullOrEmpty();
        response.Floors.Should().BeEquivalentTo(expectedFloors);
    }
}
