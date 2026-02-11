using System.Net;
using Opilo.HazardZoneMonitor.Tests.Integration.Shared;

namespace Opilo.HazardZoneMonitor.Tests.Integration;

public sealed class OpenApiSpecification(CustomWebApplicationFactory factory)
    : IClassFixture<CustomWebApplicationFactory>
{
    [Fact]
    public async Task GetOpenApiDocument_ShouldReturnOk_WhenCalled()
    {
        // Arrange
        HttpClient client = factory.CreateClient();

        // Act
        HttpResponseMessage response = await client.GetAsync(
            new Uri("/openapi/v1.json", UriKind.Relative),
            TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetOpenApiDocument_ShouldReturnJsonContentType_WhenCalled()
    {
        // Arrange
        HttpClient client = factory.CreateClient();

        // Act
        HttpResponseMessage response = await client.GetAsync(
            new Uri("/openapi/v1.json", UriKind.Relative),
            TestContext.Current.CancellationToken);

        // Assert
        response.EnsureSuccessStatusCode();
        response.Content.Headers.ContentType?.MediaType.Should().Be("application/json");
    }

    [Fact]
    public async Task GetOpenApiDocument_ShouldContainApiInfo_WhenCalled()
    {
        // Arrange
        HttpClient client = factory.CreateClient();

        // Act
        string content = await client.GetStringAsync(
            new Uri("/openapi/v1.json", UriKind.Relative),
            TestContext.Current.CancellationToken);

        // Assert
        content.Should().Contain("\"openapi\":");
        content.Should().Contain("\"paths\":");
    }

    [Fact]
    public async Task GetOpenApiDocument_ShouldIncludeMappedEndpoints_WhenCalled()
    {
        // Arrange
        HttpClient client = factory.CreateClient();

        // Act
        string content = await client.GetStringAsync(
            new Uri("/openapi/v1.json", UriKind.Relative),
            TestContext.Current.CancellationToken);

        // Assert
        content.Should().Contain("/api/v1/floors");
    }

    [Fact]
    public async Task GetScalarUi_ShouldReturnOk_WhenCalled()
    {
        // Arrange
        HttpClient client = factory.CreateClient();

        // Act
        HttpResponseMessage response = await client.GetAsync(
            new Uri("/scalar/v1", UriKind.Relative),
            TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetScalarUi_ShouldReturnHtmlContentType_WhenCalled()
    {
        // Arrange
        HttpClient client = factory.CreateClient();

        // Act
        HttpResponseMessage response = await client.GetAsync(
            new Uri("/scalar/v1", UriKind.Relative),
            TestContext.Current.CancellationToken);

        // Assert
        response.EnsureSuccessStatusCode();
        response.Content.Headers.ContentType?.MediaType.Should().Be("text/html");
    }
}
