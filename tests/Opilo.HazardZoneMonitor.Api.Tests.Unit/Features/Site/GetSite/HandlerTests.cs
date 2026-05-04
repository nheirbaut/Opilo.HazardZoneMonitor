using Ardalis.Result;
using Microsoft.Extensions.Options;
using Opilo.HazardZoneMonitor.Api.Features.Floors;
using Opilo.HazardZoneMonitor.Api.Features.Site;
using Opilo.HazardZoneMonitor.Domain.Shared.Primitives;
using Opilo.HazardZoneMonitor.Api.Features.Site.GetSite;

namespace Opilo.HazardZoneMonitor.Api.Tests.Unit.Features.Site.GetSite;

public sealed class HandlerTests
{
    [Fact]
    public async Task Handle_ShouldReturnSuccessResultWithSiteNameAndFloors_WhenSiteIsConfigured()
    {
        // Arrange
        Coordinate point1 = new(0.0, 0.0);
        Coordinate point2 = new(10.0, 10.0);
        Coordinate point3 = new(10.0, 0.0);

        FloorConfiguration floor1 = new("Floor 1", new[] { point1, point2, point3 });
        FloorConfiguration floor2 = new("Floor 2", new[] { point1, point2 });

        var siteName = SiteName.From("Test Site");
        SiteOptions siteOptions = new() { Name = siteName };
        FloorOptions floorOptions = new() { Floors = new[] { floor1, floor2 } };

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
        SiteOptions siteOptions = new() { Name = SiteName.From("Test Site") };
        FloorOptions floorOptions = new() { Floors = Array.Empty<FloorConfiguration>() };

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
