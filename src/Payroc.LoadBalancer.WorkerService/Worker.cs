namespace Payroc.LoadBalancer.WorkerService;

public class Worker(ILogger<Worker> logger) : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("Starting Payroc.LoadBalancer Service at: {TimeNow}", DateTime.UtcNow);

        logger.LogInformation("Started Payroc.LoadBalancer Service at: {TimeNow}", DateTime.UtcNow);
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("Stopping Payroc.LoadBalancer Service at: {TimeNow}", DateTime.UtcNow);

        logger.LogInformation("Stopped Payroc.LoadBalancer Service at: {TimeNow}", DateTime.UtcNow);
    }
}
