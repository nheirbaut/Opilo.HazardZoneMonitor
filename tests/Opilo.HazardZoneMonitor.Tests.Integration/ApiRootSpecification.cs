using System.Net;
using System.Net.Http.Json;
using Opilo.HazardZoneMonitor.Tests.Integration.Shared;

namespace Opilo.HazardZoneMonitor.Tests.Integration;

public sealed class ApiRootSpecification(CustomWebApplicationFactory factory)
    : IClassFixture<CustomWebApplicationFactory>
{
    [Fact]
    public async Task GetRoot_ShouldReturnOkStatusCode_WhenCalled()
    {
        // Arrange
        HttpClient client = factory.CreateClient();

        // Act
        HttpResponseMessage response = await client.GetAsync(
            new Uri("/", UriKind.Relative),
            TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetRoot_ShouldReturnJsonContentType_WhenCalled()
    {
        // Arrange
        HttpClient client = factory.CreateClient();

        // Act
        HttpResponseMessage response = await client.GetAsync(
            new Uri("/", UriKind.Relative),
            TestContext.Current.CancellationToken);

        // Assert
        response.Content.Headers.ContentType?.MediaType.Should().Be("application/json");
    }

    [Fact]
    public async Task GetRoot_ShouldIncludeApiName_WhenCalled()
    {
        // Arrange
        HttpClient client = factory.CreateClient();

        // Act
        HttpResponseMessage response = await client.GetAsync(
            new Uri("/", UriKind.Relative),
            TestContext.Current.CancellationToken);
        ApiRootResponse? apiRoot = await response.Content.ReadFromJsonAsync<ApiRootResponse>(
            cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        apiRoot?.Name.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task GetRoot_ShouldIncludeApiVersion_WhenCalled()
    {
        // Arrange
        HttpClient client = factory.CreateClient();

        // Act
        HttpResponseMessage response = await client.GetAsync(
            new Uri("/", UriKind.Relative),
            TestContext.Current.CancellationToken);
        ApiRootResponse? apiRoot = await response.Content.ReadFromJsonAsync<ApiRootResponse>(
            cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        apiRoot?.Version.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task GetRoot_ShouldIncludeLinksCollection_WhenCalled()
    {
        // Arrange
        HttpClient client = factory.CreateClient();

        // Act
        HttpResponseMessage response = await client.GetAsync(
            new Uri("/", UriKind.Relative),
            TestContext.Current.CancellationToken);
        ApiRootResponse? apiRoot = await response.Content.ReadFromJsonAsync<ApiRootResponse>(
            cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        apiRoot?.Links.Should().NotBeNull();
        apiRoot?.Links.Should().NotBeEmpty();
    }

    [Fact]
    public async Task GetRoot_ShouldIncludeFloorsLink_WhenCalled()
    {
        // Arrange
        HttpClient client = factory.CreateClient();

        // Act
        HttpResponseMessage response = await client.GetAsync(
            new Uri("/", UriKind.Relative),
            TestContext.Current.CancellationToken);
        ApiRootResponse? apiRoot = await response.Content.ReadFromJsonAsync<ApiRootResponse>(
            cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        ApiLink? floorsLink = apiRoot?.Links.FirstOrDefault(l => string.Equals(l.Rel, "floors", StringComparison.Ordinal));
        floorsLink?.Href.Should().Be("/api/v1/floors");
    }

    [Fact]
    public async Task GetRoot_ShouldIncludePersonMovementsLink_WhenCalled()
    {
        // Arrange
        HttpClient client = factory.CreateClient();

        // Act
        HttpResponseMessage response = await client.GetAsync(
            new Uri("/", UriKind.Relative),
            TestContext.Current.CancellationToken);
        ApiRootResponse? apiRoot = await response.Content.ReadFromJsonAsync<ApiRootResponse>(
            cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        ApiLink? personMovementsLink = apiRoot?.Links.FirstOrDefault(l => string.Equals(l.Rel, "person-movements", StringComparison.Ordinal));
        personMovementsLink?.Href.Should().Be("/api/v1/person-movements");
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1812",
        Justification = "Used by JSON deserialization")]
    private sealed record ApiRootResponse(string Name, string Version, List<ApiLink> Links);

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1812",
        Justification = "Used by JSON deserialization")]
    private sealed record ApiLink(string Rel, string Href);
}
