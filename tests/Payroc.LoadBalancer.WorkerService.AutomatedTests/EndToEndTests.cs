using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Payroc.LoadBalancer.WorkerService.AutomatedTests.Helpers;
using Payroc.LoadBalancer.WorkerService.AutomatedTests.Infrastructure;
using Xunit.Abstractions;

namespace Payroc.LoadBalancer.WorkerService.AutomatedTests;

[Collection(nameof(TestFixtureDependencies))]
public class EndToEndTests: WorkerServiceTestBase<LoadBalancerWebApplicationFactory,Program>
{
    private const string LoadBalancerAddress = "http://127.0.0.1:5000";
    private const string Server1Address = "http://127.0.0.1:5001";
    private const string Server2Address = "http://127.0.0.1:5002";
    
    public EndToEndTests(ITestOutputHelper output) : base(output)
    {
        EnsureServersAreHealthy().RunSync();
    }
    
    [Fact]
    public async Task RequestIsMadeThroughLoadbalancer_ThereIsServer_ResponseIsReceived()
    {
        var response = await Client.GetAsync(LoadBalancerAddress);
        response.EnsureSuccessStatusCode();
    }
    
    [Fact]
    public async Task RequestIsMadeThroughLoadbalancer_ThereIsServer_ResponseIsReceivedWithMessage()
    {
        var response = await Client.GetAsync(LoadBalancerAddress);
        
        response.EnsureSuccessStatusCode();

        var responseMessage = await response.Content.ReadAsStringAsync();
        responseMessage.Should().StartWith("Hello");
    }
    
    [Fact]
    public async Task TwoRequestsAreMadeThroughLoadbalancer_RequestsAreShared_ResponseIsReceivedWithMessage()
    {
        var response1 = await Client.GetAsync(LoadBalancerAddress);
        response1.EnsureSuccessStatusCode();

        var responseMessage1 = await response1.Content.ReadAsStringAsync();

        var response2 = await Client.GetAsync(LoadBalancerAddress);
        response2.EnsureSuccessStatusCode();

        var responseMessage2 = await response2.Content.ReadAsStringAsync();

        var set = new HashSet<string>([responseMessage1, responseMessage2]);
        set.Should().HaveCount(2);
        set.Contains("Hello from 1").Should().BeTrue();
        set.Contains("Hello from 2").Should().BeTrue();
    }
    
    [Fact]
    public async Task RequestIsMadeThroughLoadbalancer_ThereAreTwoServersOneUnhealthy_ResponseIsReceivedByHealthy()
    {
        await SetServerHealthy(Server1Address, false);
        await Task.Delay(5000); //Not pretty but we have to wait for the health service
        
        var response = await Client.GetAsync(LoadBalancerAddress);
        
        response.EnsureSuccessStatusCode();

        var responseMessage = await response.Content.ReadAsStringAsync();
        responseMessage.Should().Be("Hello from 2");
    }
    
    [Fact]
    public async Task ManyRequestsAreMadeThroughLoadbalancer_ThereIsServer_ResponseIsReceived()
    {
        var tasks = Enumerable.Range(1, 5).Select(_ => Client.GetAsync(LoadBalancerAddress));

        foreach (var task in tasks)
        {
            var response = await task;
            response.IsSuccessStatusCode.Should().BeTrue();;
        }
    }

    private async Task EnsureServersAreHealthy()
    {
        
        //var healthUri = new Uri($"http://{server.IpAddress}:{server.Port}/health");
        await SetServerHealthy(Server1Address, true);
        await SetServerHealthy(Server2Address, true);
    }

    private async Task SetServerHealthy(string serverAddress, bool health)
    {
        var h = JsonSerializer.Serialize(health);
        
        var content = new StringContent(h, System.Text.Encoding.UTF8, "application/json");
        await Client.PostAsync($"{serverAddress}/toggle-health", content);
    }
}
