using System.Net;
using System.Net.Http.Json;
using Opilo.HazardZoneMonitor.Api.Features.PersonTracking.RegisterPersonMovement;
using Opilo.HazardZoneMonitor.Tests.Integration.Shared;

namespace Opilo.HazardZoneMonitor.Tests.Integration.Features.PersonTracking;

public class GetPersonMovementsSpecification(CustomWebApplicationFactory factory)
    : IClassFixture<CustomWebApplicationFactory>
{
    [Fact]
    public async Task GetPersonMovement_ShouldReturn200Ok_WhenCalled()
    {
        // Arrange
        var client = factory.CreateClient();
        var personId = Guid.NewGuid();
        var command = new Command(personId, X: 1, Y: 1);
        var postResponse = await client.PostAsJsonAsync("/api/v1/person-movements", command, TestContext.Current.CancellationToken);
        var registration = await postResponse.Content.ReadFromJsonAsync<RegistrationResponse>(TestContext.Current.CancellationToken);

        // Act
        var response = await client.GetAsync(
            new Uri($"/api/v1/person-movements/{registration!.Id}", UriKind.Relative),
            TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetPersonMovement_ShouldReturnMovementWithRegisteredAtTimestamp_WhenMovementIsRegistered()
    {
        // Arrange
        var client = factory.CreateClient();
        var personId = Guid.NewGuid();
        var command = new Command(personId, X: 5, Y: 10);
        var postResponse = await client.PostAsJsonAsync("/api/v1/person-movements", command, TestContext.Current.CancellationToken);
        var registration = await postResponse.Content.ReadFromJsonAsync<RegistrationResponse>(TestContext.Current.CancellationToken);

        // Act
        var movement = await client.GetFromJsonAsync<PersonMovementResponse>(
            new Uri($"/api/v1/person-movements/{registration!.Id}", UriKind.Relative),
            TestContext.Current.CancellationToken);

        // Assert
        movement.Should().NotBeNull();
        movement.PersonId.Should().Be(personId);
        movement.X.Should().Be(5);
        movement.Y.Should().Be(10);
        movement.RegisteredAt.Should().NotBe(default(DateTime));
    }

    [Fact]
    public async Task GetPersonMovement_ShouldReturn404NotFound_WhenMovementDoesNotExist()
    {
        // Arrange
        var client = factory.CreateClient();
        var nonExistentId = Guid.NewGuid();

        // Act
        var response = await client.GetAsync(
            new Uri($"/api/v1/person-movements/{nonExistentId}", UriKind.Relative),
            TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    // ReSharper disable NotAccessedPositionalProperty.Local
#pragma warning disable CA1812 // Instantiated by JSON deserialization
    private sealed record RegistrationResponse(Guid Id);
    private sealed record PersonMovementResponse(Guid PersonId, double X, double Y, DateTime RegisteredAt);
#pragma warning restore CA1812
    // ReSharper restore NotAccessedPositionalProperty.Local
}
