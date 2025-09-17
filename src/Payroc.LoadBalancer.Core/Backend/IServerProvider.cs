namespace Payroc.LoadBalancer.Core.Backend;

public interface IServerProvider
{
    Server GetNextServer();
}

public interface IServerUpdater
{
    void SetServerUsed(Server server);
    void SetServerAsUnresponsive(Server server);
}