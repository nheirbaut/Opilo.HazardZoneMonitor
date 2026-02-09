using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Opilo.HazardZoneMonitor.Tests.Integration.Shared;

namespace Opilo.HazardZoneMonitor.Tests.Integration.Features.PersonTracking;

public sealed class PersonMovementPersistenceSpecification
{
    [Fact(Skip = "Movements are stored in a static field, so they survive across factory instances within the same process. This test will become meaningful once persistence is implemented (see issue #22).")]
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

            JsonElement json = await postResponse.Content
                .ReadFromJsonAsync<JsonElement>(TestContext.Current.CancellationToken);

            registrationId = json.GetProperty("id").GetGuid();
        }

        // Act
        await using CustomWebApplicationFactory secondFactory = new();
        HttpClient freshClient = secondFactory.CreateClient();

        HttpResponseMessage response = await freshClient.GetAsync(
            new Uri($"/api/v1/person-movements/{registrationId}", UriKind.Relative),
            TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
