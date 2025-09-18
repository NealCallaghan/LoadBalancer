namespace Payroc.LoadBalancer.Core.Backend;

public interface IServerUpdater
{
    void SetServerUsed(Server server);
}