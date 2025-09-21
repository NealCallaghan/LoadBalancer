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
                var servers = clusterState.ServerDictionary.ToList();

                foreach (var (server, state) in servers)
                {
                    var healthUri = new Uri($"http://{server.Address}:{server.Port}/health");

                    bool healthy;
                    try
                    {
                        var response = await client.GetAsync(healthUri, cancellationToken);
                        var healthResponse = await response.Content.ReadAsStringAsync(cancellationToken);
                        healthy = response.IsSuccessStatusCode && healthResponse == "Healthy";
                    }
                    catch(Exception ex)
                    {
                        logger.LogError(ex, "Unable to contact server");
                        healthy = false;
                    }

                    if (!healthy)
                    {
                        var unhealthyState = state with { Healthy = false };
                        clusterState.ServerDictionary.AddOrUpdate(server, unhealthyState, (_, _) => unhealthyState);
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