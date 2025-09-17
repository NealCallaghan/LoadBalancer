namespace Payroc.LoadBalancer.Core.Backend;

public interface IServerProvider
{
    IServer GetNextServer();
}