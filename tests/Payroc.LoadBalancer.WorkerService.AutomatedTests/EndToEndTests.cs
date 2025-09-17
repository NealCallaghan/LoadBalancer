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
        const string loadBalancerAddress = "http://localhost:8081";
        
        var response = await Client.GetAsync(loadBalancerAddress);
        response.EnsureSuccessStatusCode();
    }
}
