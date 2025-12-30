using System.Net;
using System.Net.Http.Json;
using Microsoft.Extensions.Configuration;
using Opilo.HazardZoneMonitor.Api.Features.FloorManagement;
using Opilo.HazardZoneMonitor.Tests.Integration.Shared;

namespace Opilo.HazardZoneMonitor.Tests.Integration.Features.FloorManagement;

public sealed class GetFloorsSpecification(CustomWebApplicationFactory factory)
    : IClassFixture<CustomWebApplicationFactory>
{
    [Fact]
    public async Task GetFloors_ShouldReturn200Ok_WhenCalled()
    {
        // Arrange
        var client = factory.CreateClient();

        // Act
        var response = await client.GetAsync(new Uri("/api/v1/floors", UriKind.Relative));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetFloors_ShouldSendResponseWithoutFloors_WhenNoFloorsAreRegistered()
    {
        // Arrange
        var client = factory.CreateClient();

        // Act
        var response = await client.GetFromJsonAsync<GetFloorResponse>(new Uri("/api/v1/floors", UriKind.Relative));

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
            new()
            {
                Name = "First Floor",
                Outline = new List<FloorPointConfiguration>
                {
                    new() { X = 0, Y = 0 },
                    new() { X = 10, Y = 0 },
                    new() { X = 10, Y = 10 },
                    new() { X = 0, Y = 10 }
                }
            },
            new()
            {
                Name = "Second Floor",
                Outline = new List<FloorPointConfiguration>
                {
                    new() { X = 0, Y = 0 },
                    new() { X = 15, Y = 0 },
                    new() { X = 15, Y = 15 },
                    new() { X = 0, Y = 15 }
                }
            }
        ];
        var floorOptions = new FloorOptions { Floors = expectedFloors };

        using var customFactory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureAppConfiguration((_, config) =>
            {
                config.AddInMemoryCollection(floorOptions.ToConfigurationDictionary());
            });
        });

        var client = customFactory.CreateClient();

        // Act
        var response = await client.GetFromJsonAsync<GetFloorResponse>(new Uri("/api/v1/floors", UriKind.Relative));

        // Assert
        response.Should().NotBeNull();
        response.Floors.Should().NotBeNullOrEmpty();
        response.Floors.Should().BeEquivalentTo(expectedFloors);
    }
}
