using Payroc.LoadBalancer.WorkerService.AutomatedTests.Helpers;

namespace Payroc.LoadBalancer.WorkerService.AutomatedTests.Infrastructure;

public class TestFixtureDependencies : IDisposable
{
    private readonly InfrastructureServicesContainer _container;

    public TestFixtureDependencies()
    {
        _container = new InfrastructureServicesContainer();
        _container.InitializeAsync().RunSync();
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected void Dispose(bool disposing)
    {
        if (disposing)
        {
            _container.DisposeAsync().Wait();
        }
    }
}
