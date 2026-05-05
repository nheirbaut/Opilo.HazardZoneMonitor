using Ardalis.Result;
using NSubstitute;
using Opilo.HazardZoneMonitor.Api.Features.HazardZones.ActivateHazardZone;
using Opilo.HazardZoneMonitor.Api.Features.HazardZones.Services;
using Opilo.HazardZoneMonitor.Domain.Shared.Primitives;

namespace Opilo.HazardZoneMonitor.Api.Tests.Unit.Features.HazardZones.ActivateHazardZone;

public sealed class HandlerTests
{
    [Fact]
    public async Task Handle_ShouldReturnNotFoundResult_WhenNoHazardZonesAreConfigured()
    {
        // Arrange
        var hazardZoneService = Substitute.For<IHazardZoneService>();
        hazardZoneService
            .ActivateHazardZone(Arg.Any<HazardZoneName>())
            .Returns(Result.NotFound());

        Handler handler = new(hazardZoneService);
        Command command = new(HazardZoneName.From("non-existing-hazardzone"));

        // Act
        var result = await handler.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        result.Status.Should().Be(ResultStatus.NotFound);
    }
}
