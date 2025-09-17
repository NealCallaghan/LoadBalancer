namespace Payroc.LoadBalancer.Core;

public interface IServerProvider
{
    IServer GetNextServer();
}