using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
using Payroc.LoadBalancer.Core.DependencyInjection.Options;
using Payroc.LoadBalancer.Core.Exceptions;

namespace Payroc.LoadBalancer.Core.Backend;

public sealed class ServerProvider : IServerProvider, IServerUpdater
{
    private readonly ILogger<ServerProvider> _logger;
    private readonly LoadBalancerOptions _loadBalancerOptions;

    private readonly ConcurrentDictionary<Server, Server> _servers;
    
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
    
    public async Task<Server> GetNextServer(CancellationToken cancellationToken)
    {
        _logger.LogDebug("Getting next server");
        
        while (!cancellationToken.IsCancellationRequested)
        {
            var resultServer = _servers
                .Values
                .OrderBy(x => x.State.TimesUsed) // This will do for now
                .FirstOrDefault(x => x.State.Healthy);

            if (resultServer != null)
            {
                return  resultServer;
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

    public void SetServerUsed(Server server)
    {
        var newServerState = server.State with { TimesUsed = server.State.TimesUsed + 1 };
        var newServer = server with { State = newServerState };
        
        _servers.AddOrUpdate(server, newServer, (_, _) => newServer);
        
        _logger.LogDebug("Marked server {Server} as used.", server);
    }
}