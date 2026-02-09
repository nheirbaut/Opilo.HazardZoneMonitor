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

    [Fact]
    public async Task GetPersonMovements_ShouldReturnMovementsWithRegisteredAtTimestamp_WhenMovementsAreRegistered()
    {
        // Arrange
        var client = factory.CreateClient();
        var personId = Guid.NewGuid();
        var command = new Command(personId, X: 5, Y: 10);
        await client.PostAsJsonAsync("/api/v1/person-movements", command, TestContext.Current.CancellationToken);

        // Act
        var response = await client.GetFromJsonAsync<List<PersonMovementResponse>>(
            new Uri($"/api/v1/person-movements/{personId}", UriKind.Relative),
            TestContext.Current.CancellationToken);

        // Assert
        response.Should().NotBeNull();
        response.Should().NotBeEmpty();
        response.Should().HaveCount(1);
        var movement = response[0];
        movement.PersonId.Should().Be(personId);
        movement.X.Should().Be(5);
        movement.Y.Should().Be(10);
        movement.RegisteredAt.Should().NotBe(default(DateTime));
    }

    // ReSharper disable NotAccessedPositionalProperty.Local
#pragma warning disable CA1812 // Instantiated by JSON deserialization
    private sealed record PersonMovementResponse(Guid PersonId, double X, double Y, DateTime RegisteredAt);
#pragma warning restore CA1812
    // ReSharper restore NotAccessedPositionalProperty.Local

}
