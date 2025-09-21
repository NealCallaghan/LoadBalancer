using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
using Payroc.LoadBalancer.Core.DependencyInjection.Options;
using Payroc.LoadBalancer.Core.Exceptions;

namespace Payroc.LoadBalancer.Core.Backend;

public sealed class ServerProvider : IServerProvider, IServerUpdater
{
    private readonly ILogger<ServerProvider> _logger;
    private readonly LoadBalancerOptions _loadBalancerOptions;

    private readonly ConcurrentDictionary<ServerAddressAndPort, ServerState> _servers;
    
    public ServerProvider(ILogger<ServerProvider> logger, ClusterState clusterState, LoadBalancerOptions loadBalancerOptions)
    {
        _logger = logger;
        _loadBalancerOptions = loadBalancerOptions;
        _servers = clusterState.ServerDictionary;
        if (!_servers.Any())
        {
            throw new NoAvailableServersException("No servers configured");
        }
    }
    
    public async Task<ServerAddressAndPort> GetNextServer(CancellationToken cancellationToken)
    {
        _logger.LogDebug("Getting next server");
        
        while (!cancellationToken.IsCancellationRequested)
        {
            var (addressAndPort, _) = _servers
                .OrderBy(x => x.Value.TimesUsed)
                .FirstOrDefault(x => x.Value.Healthy);
            
            if (addressAndPort != null)
            {
                return addressAndPort;
            }
            
            _logger.LogDebug(
                "Could not find a healthy server. Waiting for the next available in {milliseconds} milliseconds", 
                _loadBalancerOptions.MillisecondsBetweenRequestsIfBusy);
            
            // here I would probably thinking about an alerting service of some sort if nothing
            // was found after a retry count. 
            await Task.Delay(TimeSpan.FromMilliseconds(_loadBalancerOptions.MillisecondsBetweenRequestsIfBusy), cancellationToken);
        }
        
        throw new OperationCanceledException("Request cancelled while waiting for a healthy server.");
    }

    public void SetServerUsed(ServerAddressAndPort server)
    {
        _servers.TryGetValue(server, out var serverState);

        var newServerState = serverState! with { TimesUsed = serverState.TimesUsed + 1 };
        
        _servers.AddOrUpdate(server, newServerState, (_, _) => newServerState);
        
        _logger.LogDebug("Marked server {Server} as used.", server);
    }
}