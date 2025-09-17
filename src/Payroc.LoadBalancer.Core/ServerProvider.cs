using Microsoft.Extensions.Logging;
using Payroc.LoadBalancer.Core.DependencyInjection.Options;

namespace Payroc.LoadBalancer.Core;

public sealed class ServerProvider : IServerProvider
{
    private readonly ILogger<ServerProvider> _logger;
    private readonly IReadOnlyCollection<IServer>  _servers;
    
    public ServerProvider(ILogger<ServerProvider> logger, IReadOnlyCollection<IServer> servers)
    {
        _logger = logger;
        if (servers.Any())
        {
            throw new ApplicationException("No servers configured");
        }
        
        _servers = servers;
    }
    
    public IServer GetNextServer()
    {
        return _servers.First();
    }
}