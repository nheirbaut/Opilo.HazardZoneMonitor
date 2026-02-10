using System.Net;
using System.Net.Http.Json;
using Opilo.HazardZoneMonitor.Api.Features.PersonTracking;
using Opilo.HazardZoneMonitor.Api.Features.PersonTracking.RegisterPersonMovement;
using Opilo.HazardZoneMonitor.Tests.Integration.Shared;

namespace Opilo.HazardZoneMonitor.Tests.Integration.Features.PersonTracking;

public class RegisterPersonMovementsSpecification(CustomWebApplicationFactory factory)
    : IClassFixture<CustomWebApplicationFactory>
{
    [Fact]
    public async Task RegisterPersonMovement_ShouldReturn201Created_WhenCalled()
    {
        // Arrange
        var client = factory.CreateClient();
        var personId = Guid.NewGuid();
        var request = new Command(personId, X: 1, Y: 1);

        // Act
        var response = await client.PostAsJsonAsync("/api/v1/person-movements", request, TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    [Fact]
    public async Task RegisterPersonMovement_ShouldIncludeRegisteredAtTimestamp_WhenCalled()
    {
        // Arrange
        var client = factory.CreateClient();
        var personId = Guid.NewGuid();
        var request = new Command(personId, X: 1, Y: 1);

        // Act
        var response = await client.PostAsJsonAsync("/api/v1/person-movements", request, TestContext.Current.CancellationToken);

        // Assert
        var registeredPersonMovement = await response.Content.ReadFromJsonAsync<RegisteredPersonMovement>(TestContext.Current.CancellationToken);
        var registeredAt = registeredPersonMovement?.RegisteredAt;
        registeredAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task RegisterPersonMovement_ShouldReturnLocationHeader_WhenCalled()
    {
        // Arrange
        var client = factory.CreateClient();
        var personId = Guid.NewGuid();
        var request = new Command(personId, X: 1, Y: 1);

        // Act
        var response = await client.PostAsJsonAsync("/api/v1/person-movements", request, TestContext.Current.CancellationToken);

        // Assert
        var registeredPersonMovement = await response.Content.ReadFromJsonAsync<RegisteredPersonMovement>(TestContext.Current.CancellationToken);
        var id = registeredPersonMovement?.Id;
        response.Headers.Location.Should().NotBeNull();
        response.Headers.Location!.ToString().Should().Be($"/api/v1/person-movements/{id}");
    }
}
