using Ardalis.Result;
using Microsoft.Extensions.Options;
using Opilo.HazardZoneMonitor.Api.Features.HazardZones.ActivateHazardZone;
using Opilo.HazardZoneMonitor.Api.Features.HazardZones.Configuration;
using Opilo.HazardZoneMonitor.Domain.Shared.Primitives;

namespace Opilo.HazardZoneMonitor.Api.Tests.Unit.Features.HazardZones.ActivateHazardZone;

public sealed class HandlerTests
{
    [Fact]
    public async Task Handle_ShouldReturnNotFoundResult_WhenNoHazardZonesAreConfigured()
    {
        // Arrange
        HazardZoneOptions hazardZoneOptions = new()
        {
            HazardZones = []
        };

        var options = Options.Create(hazardZoneOptions);
        Handler handler = new(options);
        Command command = new(HazardZoneName.From("non-existing-hazardzone"));

        // Act
        var result = await handler.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        result.Status.Should().Be(ResultStatus.NotFound);
    }
}
