using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Opilo.HazardZoneMonitor.Api;
using Opilo.HazardZoneMonitor.Api.Features.FloorManagement;

namespace Opilo.HazardZoneMonitor.Tests.Integration.Features.FloorManagement;

public sealed class GetFloorsEndpointTests : IDisposable
{
    private readonly CustomWebApplicationFactory _factory;

    public GetFloorsEndpointTests()
    {
        _factory = new CustomWebApplicationFactory(new FakeFloorRegistry([]));
    }

    [Fact]
    public async Task GetFloors_ShouldReturn200_WithEmptyArray()
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act
        var response = await client.GetAsync(new Uri("/api/v1/floors", UriKind.Relative));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Be("[]");
    }

    [Fact]
    public async Task GetFloors_ShouldReturnApplicationJson()
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act
        var response = await client.GetAsync(new Uri("/api/v1/floors", UriKind.Relative));

        // Assert
        response.Content.Headers.ContentType?.MediaType.Should().Be("application/json");
    }

    [Fact]
    public async Task GetFloors_ShouldReturnSingleFloor_WhenFloorExists()
    {
        // Arrange
        var expectedFloors = new[] { new FloorDto("floor-1", "Ground Floor") };
        await using var factory = new CustomWebApplicationFactory(new FakeFloorRegistry(expectedFloors));
        var client = factory.CreateClient();

        // Act
        var floors = await client.GetFromJsonAsync<FloorDto[]>("/api/v1/floors");

        // Assert
        floors.Should().BeEquivalentTo(expectedFloors);
    }

    public void Dispose()
    {
        _factory.Dispose();
    }
}

internal sealed class FakeFloorRegistry : IFloorRegistry
{
    private readonly IReadOnlyList<FloorDto> _floors;

    public FakeFloorRegistry(IReadOnlyList<FloorDto> floors)
    {
        _floors = floors;
    }

    public IReadOnlyList<FloorDto> GetAllFloors() => _floors;
}

internal sealed class CustomWebApplicationFactory : WebApplicationFactory<IApiMarker>
{
    private readonly IFloorRegistry _floorRegistry;

    public CustomWebApplicationFactory(IFloorRegistry floorRegistry)
    {
        _floorRegistry = floorRegistry;
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            services.AddSingleton(_floorRegistry);
        });
    }
}
