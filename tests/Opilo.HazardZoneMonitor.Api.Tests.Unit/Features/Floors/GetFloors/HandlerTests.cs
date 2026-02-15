using Ardalis.Result;
using Microsoft.Extensions.Options;
using Opilo.HazardZoneMonitor.Api.Features.Floors;
using Opilo.HazardZoneMonitor.Api.Features.Floors.GetFloors;
using Opilo.HazardZoneMonitor.Api.Shared.Configuration;

namespace Opilo.HazardZoneMonitor.Api.Tests.Unit.Features.Floors.GetFloors;

public sealed class HandlerTests
{
    [Fact]
    public async Task Handle_ShouldReturnSuccessResult_WhenFloorsAreConfigured()
    {
        // Arrange
        PointConfiguration point1 = new(0.0, 0.0);
        PointConfiguration point2 = new(10.0, 10.0);
        PointConfiguration point3 = new(10.0, 0.0);

        FloorConfiguration floor1 = new("Floor 1", new[] { point1, point2, point3 });
        FloorConfiguration floor2 = new("Floor 2", new[] { point1, point2 });

        FloorOptions floorOptions = new()
        {
            Floors = new[] { floor1, floor2 },
        };

        IOptions<FloorOptions> options = Options.Create(floorOptions);
        Handler handler = new(options);
        Query query = new();

        // Act
        Result<Response> result = await handler.Handle(query, TestContext.Current.CancellationToken);

        // Assert
        result.Status.Should().Be(ResultStatus.Ok);
        result.Value.Floors.Should().BeEquivalentTo(new[] { floor1, floor2 });
    }

    [Fact]
    public async Task Handle_ShouldReturnSuccessResultWithEmptyFloors_WhenNoFloorsAreConfigured()
    {
        // Arrange
        FloorOptions floorOptions = new()
        {
            Floors = Array.Empty<FloorConfiguration>(),
        };

        IOptions<FloorOptions> options = Options.Create(floorOptions);
        Handler handler = new(options);
        Query query = new();

        // Act
        Result<Response> result = await handler.Handle(query, TestContext.Current.CancellationToken);

        // Assert
        result.Status.Should().Be(ResultStatus.Ok);
        result.Value.Floors.Should().BeEmpty();
    }
}
