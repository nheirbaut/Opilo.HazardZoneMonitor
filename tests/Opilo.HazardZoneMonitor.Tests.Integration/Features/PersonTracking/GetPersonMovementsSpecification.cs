using System.Net;
using System.Net.Http.Json;
using Opilo.HazardZoneMonitor.Api.Features.PersonTracking.RegisterPersonMovement;
using Opilo.HazardZoneMonitor.Tests.Integration.Shared;

namespace Opilo.HazardZoneMonitor.Tests.Integration.Features.PersonTracking;

public class GetPersonMovementsSpecification(CustomWebApplicationFactory factory)
    : IClassFixture<CustomWebApplicationFactory>
{
    [Fact]
    public async Task GetPersonMovements_ShouldReturn200Ok_WhenCalled()
    {
        // Arrange
        var client = factory.CreateClient();
        var personId = Guid.NewGuid();
        var command = new Command(personId, X: 1, Y: 1);
        await client.PostAsJsonAsync("/api/v1/person-movements", command, TestContext.Current.CancellationToken);

        // Act
        var response = await client.GetAsync(
            new Uri($"/api/v1/person-movements/{personId}", UriKind.Relative),
            TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
