using Microsoft.Extensions.Logging;

namespace Payroc.LoadBalancer.Core.Backend;

public sealed class ServerProvider : IServerProvider, IServerUpdater
{
    private readonly ILogger<ServerProvider> _logger;
    private readonly IReadOnlyCollection<Server>  _servers;
    
    public ServerProvider(ILogger<ServerProvider> logger, IReadOnlyCollection<Server> servers)
    {
        _logger = logger;
        if (!servers.Any())
        {
            throw new ApplicationException("No servers configured");
        }
        
        _servers = servers;
    }
    
    public Server GetNextServer()
    {
        return _servers.First();
    }

    public void SetServerUsed(Server server)
    {
        throw new NotImplementedException();
    }

    public void SetServerAsUnresponsive(Server server)
    {
        throw new NotImplementedException();
    }
}