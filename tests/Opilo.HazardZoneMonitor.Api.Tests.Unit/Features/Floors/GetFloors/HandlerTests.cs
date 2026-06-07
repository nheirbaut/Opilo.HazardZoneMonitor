using Ardalis.Result;
using Microsoft.Extensions.Options;
using Opilo.HazardZoneMonitor.Api.Features.Floors.Configuration;
using Opilo.HazardZoneMonitor.Domain.Shared.Primitives;
using Opilo.HazardZoneMonitor.Api.Features.Floors.GetFloors;
using Opilo.HazardZoneMonitor.Tests.Common.TestUtilities.Builders;

namespace Opilo.HazardZoneMonitor.Api.Tests.Unit.Features.Floors.GetFloors;

public sealed class HandlerTests
{
    [Fact]
    public async Task Handle_ShouldReturnSuccessResult_WhenFloorsAreConfigured()
    {
        // Arrange
        var floorOptions = FloorOptionsBuilder.Create()
            .WithFloor("Floor 1", f => f.WithOutline(new Coordinate(0, 0), new Coordinate(10, 10), new Coordinate(10, 0)))
            .WithFloor("Floor 2", f => f.WithOutline(new Coordinate(0, 0), new Coordinate(10, 10)))
            .Build();

        var floor1 = floorOptions.Floors[0];
        var floor2 = floorOptions.Floors[1];

        IOptions<FloorOptions> options = Options.Create(floorOptions);
        Handler handler = new(options);
        Query query = new();

        // Act
        Result<GetFloorsResponse> result = await handler.Handle(query, TestContext.Current.CancellationToken);

        // Assert
        result.Status.Should().Be(ResultStatus.Ok);
        result.Value.Floors.Should().BeEquivalentTo(new[] { floor1, floor2 });
    }

    [Fact]
    public async Task Handle_ShouldReturnSuccessResultWithEmptyFloors_WhenNoFloorsAreConfigured()
    {
        // Arrange
        FloorOptions floorOptions = FloorOptionsBuilder.Create().Build();

        IOptions<FloorOptions> options = Options.Create(floorOptions);
        Handler handler = new(options);
        Query query = new();

        // Act
        Result<GetFloorsResponse> result = await handler.Handle(query, TestContext.Current.CancellationToken);

        // Assert
        result.Status.Should().Be(ResultStatus.Ok);
        result.Value.Floors.Should().BeEmpty();
    }
}
