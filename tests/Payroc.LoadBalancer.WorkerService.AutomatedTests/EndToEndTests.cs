using Payroc.LoadBalancer.WorkerService.AutomatedTests.Helpers;
using Payroc.LoadBalancer.WorkerService.AutomatedTests.Infrastructure;
using Xunit.Abstractions;

namespace Payroc.LoadBalancer.WorkerService.AutomatedTests;

[Collection(nameof(TestFixtureDependencies))]
public class EndToEndTests(ITestOutputHelper output) : WorkerServiceTestBase<LoadBalancerWebApplicationFactory,Program>(output)
{
    
}
