namespace Opilo.HazardZoneMonitor.Tests.Integration;

public sealed class ServerStartupSpecification(CustomWebApplicationFactory factory)
    : IClassFixture<CustomWebApplicationFactory>
{
    [Fact]
    public void Api_ShouldFailToStart_WhenSiteConfigurationIsInvalid()
    {
        // Arrange
        var hostBuilder = factory.CreateHost()
            .WithSetting("SiteOptions:Name", string.Empty);

        // Act
        Action act = () => hostBuilder.Start();

        // Assert
        act.Should().Throw<InvalidOperationException>();
    }
}
