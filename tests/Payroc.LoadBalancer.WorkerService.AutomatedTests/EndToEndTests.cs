using FluentAssertions;
using Payroc.LoadBalancer.WorkerService.AutomatedTests.Helpers;
using Payroc.LoadBalancer.WorkerService.AutomatedTests.Infrastructure;
using Xunit.Abstractions;

namespace Payroc.LoadBalancer.WorkerService.AutomatedTests;

[Collection(nameof(TestFixtureDependencies))]
public class EndToEndTests(ITestOutputHelper output) : WorkerServiceTestBase<LoadBalancerWebApplicationFactory,Program>(output)
{
    [Fact]
    public async Task RequestIsMadeThroughLoadbalancer_ThereIsServer_ResponseIsReceived()
    {
        const string loadBalancerAddress = "http://127.0.0.1:5000";
        
        var response = await Client.GetAsync(loadBalancerAddress);
        response.EnsureSuccessStatusCode();
    }
    
    [Fact]
    public async Task ManyRequestsAreMadeThroughLoadbalancer_ThereIsServer_ResponseIsReceived()
    {
        const string loadBalancerAddress = "http://127.0.0.1:5000";
        
        var tasks = Enumerable.Range(1, 5).Select(_ => Client.GetAsync(loadBalancerAddress));
        var responses = await Task.WhenAll(tasks);

        foreach (var response in responses)
        {
            response.IsSuccessStatusCode.Should().BeTrue();;
        }
    }
}
