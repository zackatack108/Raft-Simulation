using FluentAssertions;
using System.Text.Json;

namespace GatewayAPITests;

public class Tests
{
    private HttpClient client;

    [OneTimeSetUp]
    public void Setup()
    {
        client = new HttpClient();
        client.BaseAddress = new Uri("http://zack-test-gateway-api:8080");
    }

    [Test]
    public async Task PingNodes()
    {
        var response = await client.GetAsync("/Gateway/PingNodes");

        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync();
        List<string> nodeList = JsonSerializer.Deserialize<List<string>>(content);

        nodeList.Count.Should().Be(3);
        nodeList[0].Should().Be("zack-test-node-api-1:8080: Pong");
        nodeList[1].Should().Be("zack-test-node-api-2:8080: Pong");
        nodeList[2].Should().Be("zack-test-node-api-3:8080: Pong");

    }
}