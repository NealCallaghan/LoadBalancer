using Microsoft.Extensions.Logging;
using Payroc.LoadBalancer.Core.Backend;
using Payroc.LoadBalancer.Core.DependencyInjection.Options;

namespace Payroc.LoadBalancer.Core.Services;

public class HealthChecker(
    ILogger<HealthChecker> logger, 
    ClusterState clusterState, 
    IHttpClientFactory httpClientFactory,
    HealthServiceOptions options) : IHeathChecker
{
    public async Task Initialize(CancellationToken cancellationToken)
    {
        logger.LogInformation("Health check service started at: {TimeNow}", DateTime.UtcNow);

        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                var client = httpClientFactory.CreateClient();
                var servers = clusterState.ServerDictionary.Keys.ToList();

                foreach (var server in servers)
                {
                    var healthUri = new Uri($"http://{server.IpAddress}:{server.Port}/health");

                    bool healthy;
                    try
                    {
                        var response = await client.GetAsync(healthUri, cancellationToken);
                        var healthResponse = await response.Content.ReadAsStringAsync(cancellationToken);
                        healthy = response.IsSuccessStatusCode && healthResponse == "Healthy";
                    }
                    catch
                    {
                        healthy = false;
                    }

                    if (!healthy)
                    {
                        var unhealthyState = server.State with { Healthy = false };
                        var unhealthyServer = new Server(server.IpAddress, server.Port, unhealthyState);
                        clusterState.ServerDictionary.AddOrUpdate(server, unhealthyServer, (_,_)  => unhealthyServer);
                    }
                }
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                logger.LogError(ex, "Error during health check loop.");
            }

            await Task.Delay(TimeSpan.FromSeconds(options.HealthServiceDelaySeconds), cancellationToken);
        }
    }
}