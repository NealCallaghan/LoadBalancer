namespace Payroc.LoadBalancer.Core.Backend;

public interface IServerProvider
{
    Task<Server> GetNextServer(CancellationToken cancellationToken);
}