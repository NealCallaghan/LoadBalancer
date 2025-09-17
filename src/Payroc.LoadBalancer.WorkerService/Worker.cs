using Payroc.LoadBalancer.Core;
using Payroc.LoadBalancer.Core.Services;

namespace Payroc.LoadBalancer.WorkerService;

public class Worker(ILogger<Worker> logger, ILoadBalancer loadBalancer) : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("Starting Payroc.LoadBalancer Service at: {TimeNow}", DateTime.UtcNow);
        
        await loadBalancer.Start(cancellationToken);
        
        logger.LogInformation("Started Payroc.LoadBalancer Service at: {TimeNow}", DateTime.UtcNow);
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("Stopping Payroc.LoadBalancer Service at: {TimeNow}", DateTime.UtcNow);
        
        await loadBalancer.Stop();
        
        logger.LogInformation("Stopped Payroc.LoadBalancer Service at: {TimeNow}", DateTime.UtcNow);
    }
}
