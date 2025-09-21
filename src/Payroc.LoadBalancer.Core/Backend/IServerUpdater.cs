namespace Payroc.LoadBalancer.Core.Backend;

public interface IServerUpdater
{
    void SetServerUsed(ServerAddressAndPort server);
}