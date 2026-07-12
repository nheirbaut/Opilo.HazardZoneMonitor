using Ardalis.Result;
using Microsoft.Extensions.Options;
using Opilo.HazardZoneMonitor.Domain.Shared.Primitives;
using Opilo.HazardZoneMonitor.Api.Features.Site.GetSite;
using Opilo.HazardZoneMonitor.Tests.Common.TestUtilities.Builders;

namespace Opilo.HazardZoneMonitor.Api.Tests.Unit.Features.Site.GetSite;

public sealed class HandlerTests
{
    [Fact]
    public async Task Handle_ShouldReturnSuccessResultWithSiteNameAndFloors_WhenSiteIsConfigured()
    {
        // Arrange
        var siteName = SiteName.From("Test Site");
        var floorOptions = FloorOptionsBuilder.Create()
            .WithFloor("Floor 1", f => f.WithOutline(new Coordinate(0, 0), new Coordinate(10, 10), new Coordinate(10, 0)))
            .WithFloor("Floor 2", f => f.WithOutline(new Coordinate(0, 0), new Coordinate(10, 10)))
            .Build();

        var siteOptions = SiteOptionsBuilder.Create().WithName("Test Site").Build();

        var floor1 = floorOptions.Floors[0];
        var floor2 = floorOptions.Floors[1];

        var siteOpts = Options.Create(siteOptions);
        var floorOpts = Options.Create(floorOptions);
        Handler handler = new(siteOpts, floorOpts);
        Query query = new();

        // Act
        var result = await handler.Handle(query, TestContext.Current.CancellationToken);

        // Assert
        result.Status.Should().Be(ResultStatus.Ok);
        result.Value.Site.Name.Should().Be(siteName.Value);
        result.Value.Site.Floors.Should().BeEquivalentTo(new[] { floor1, floor2 });
    }

    [Fact]
    public async Task Handle_ShouldReturnSuccessResultWithEmptyFloors_WhenNoFloorsAreConfigured()
    {
        // Arrange
        var siteOptions = SiteOptionsBuilder.Create().WithName("Test Site").Build();
        var floorOptions = FloorOptionsBuilder.Create().Build();

        var siteOpts = Options.Create(siteOptions);
        var floorOpts = Options.Create(floorOptions);
        Handler handler = new(siteOpts, floorOpts);
        Query query = new();

        // Act
        var result = await handler.Handle(query, TestContext.Current.CancellationToken);

        // Assert
        result.Status.Should().Be(ResultStatus.Ok);
        result.Value.Site.Floors.Should().BeEmpty();
    }
}
