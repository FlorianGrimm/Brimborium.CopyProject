namespace Brimborium.CreateProject.Tests.TestUtility;

public class TestsIntegration {
    [ClassDataSource<WebApplicationFactoryIntegration>(Shared = SharedType.PerTestSession)]
    public required WebApplicationFactoryIntegration WebApplicationFactory { get; init; }

    [Test]
    public async Task PingTest() {
        var client = this.WebApplicationFactory.CreateClient();

        var response = await client.GetAsync("/ping");

        var stringContent = await response.Content.ReadAsStringAsync();

        await Assert.That(stringContent).StartsWith("pong ");
    }
}
