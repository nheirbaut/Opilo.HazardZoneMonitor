using System.Net;
using System.Net.Http.Json;
using Opilo.HazardZoneMonitor.Api.Features.PersonTracking.GetRegisteredPersonMovement;
using Opilo.HazardZoneMonitor.Api.Features.PersonTracking.RegisterPersonMovement;
using Opilo.HazardZoneMonitor.Domain.Shared.Primitives;
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
        var personId = PersonId.From(Guid.NewGuid());
        var request = new Command(personId, new Coordinate(1, 1));

        // Act
        var response = await client.PostAsJsonAsync("/api/v1/person-movements", request, SerializationOptions.Default, TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    [Fact]
    public async Task RegisterPersonMovement_ShouldIncludeRegisteredAtTimestamp_WhenCalled()
    {
        // Arrange
        var client = factory.CreateClient();
        var personId = PersonId.From(Guid.NewGuid());
        var request = new Command(personId, new Coordinate(1, 1));

        // Act
        var response = await client.PostAsJsonAsync("/api/v1/person-movements", request, SerializationOptions.Default, TestContext.Current.CancellationToken);

        // Assert
        var registeredPersonMovement = await response.Content.ReadFromJsonAsync<RegisteredPersonMovement>(SerializationOptions.Default, TestContext.Current.CancellationToken);
        registeredPersonMovement.Should().NotBeNull();
        registeredPersonMovement.RegisteredAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task RegisterPersonMovement_ShouldReturnLocationHeader_WhenCalled()
    {
        // Arrange
        var client = factory.CreateClient();
        var personId = PersonId.From(Guid.NewGuid());
        var request = new Command(personId, new Coordinate(1, 1));

        // Act
        var response = await client.PostAsJsonAsync("/api/v1/person-movements", request, SerializationOptions.Default, TestContext.Current.CancellationToken);

        // Assert
        var registeredPersonMovement = await response.Content.ReadFromJsonAsync<RegisteredPersonMovement>(SerializationOptions.Default, TestContext.Current.CancellationToken);
        registeredPersonMovement.Should().NotBeNull();
        response.Headers.Location.Should().NotBeNull();
        response.Headers.Location!.ToString().Should().Be($"/api/v1/person-movements/{registeredPersonMovement.Id}");
    }
}
