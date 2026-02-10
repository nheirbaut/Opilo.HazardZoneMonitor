using System.Net;
using System.Net.Http.Json;
using Opilo.HazardZoneMonitor.Api.Features.PersonTracking;
using Opilo.HazardZoneMonitor.Tests.Integration.Shared;

namespace Opilo.HazardZoneMonitor.Tests.Integration.Features.PersonTracking;

public sealed class PersonMovementPersistenceSpecification
{
    [Fact]
    public async Task GetPersonMovement_ShouldReturnMovement_WhenApplicationIsRestartedAfterRegistration()
    {
        // Arrange
        Guid registrationId;

        await using (CustomWebApplicationFactory firstFactory = new())
        {
            HttpClient client = firstFactory.CreateClient();

            HttpResponseMessage postResponse = await client.PostAsJsonAsync(
                "/api/v1/person-movements",
                new { PersonId = Guid.NewGuid(), X = 5.0, Y = 10.0 },
                TestContext.Current.CancellationToken);

            RegisteredPersonMovement? movementRegistration = await postResponse.Content
                .ReadFromJsonAsync<RegisteredPersonMovement>(TestContext.Current.CancellationToken);

            registrationId = movementRegistration!.Id;
        }

        // Act
        await using CustomWebApplicationFactory secondFactory = new();
        var freshClient = secondFactory.CreateClient();

        HttpResponseMessage response = await freshClient.GetAsync(
            new Uri($"/api/v1/person-movements/{registrationId}", UriKind.Relative),
            TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
