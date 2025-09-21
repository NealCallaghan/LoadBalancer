namespace Payroc.LoadBalancer.Core.Backend;

public interface IServerProvider
{
    Task<ServerAddressAndPort> GetNextServer(CancellationToken cancellationToken);
}