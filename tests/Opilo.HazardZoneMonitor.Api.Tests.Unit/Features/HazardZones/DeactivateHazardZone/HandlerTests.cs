using Ardalis.Result;
using NSubstitute;
using Opilo.HazardZoneMonitor.Api.Features.HazardZones.DeactivateHazardZone;
using Opilo.HazardZoneMonitor.Api.Features.HazardZones.Services;
using Opilo.HazardZoneMonitor.Domain.Shared.Primitives;

namespace Opilo.HazardZoneMonitor.Api.Tests.Unit.Features.HazardZones.DeactivateHazardZone;

public sealed class HandlerTests
{
    [Fact]
    public async Task Handle_ShouldReturnOkResult_WhenHazardZoneServiceSucceeds()
    {
        // Arrange
        var hazardZoneService = Substitute.For<IHazardZoneService>();
        hazardZoneService
            .DeactivateHazardZone(Arg.Any<HazardZoneName>())
            .Returns(Result.Success());

        Handler handler = new(hazardZoneService);
        Command command = new(HazardZoneName.From("existing-hazardzone"));

        // Act
        var result = await handler.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        result.Status.Should().Be(ResultStatus.Ok);
    }

    [Fact]
    public async Task Handle_ShouldReturnNotFoundResult_WhenNoHazardZonesAreConfigured()
    {
        // Arrange
        var hazardZoneService = Substitute.For<IHazardZoneService>();
        hazardZoneService
            .DeactivateHazardZone(Arg.Any<HazardZoneName>())
            .Returns(Result.NotFound());

        Handler handler = new(hazardZoneService);
        Command command = new(HazardZoneName.From("non-existing-hazardzone"));

        // Act
        var result = await handler.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        result.Status.Should().Be(ResultStatus.NotFound);
    }
}
