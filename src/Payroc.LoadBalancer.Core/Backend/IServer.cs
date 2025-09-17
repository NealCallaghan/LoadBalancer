using System.Net;

namespace Payroc.LoadBalancer.Core.Backend;

public interface IServer
{
    public IPAddress IpAddress { get; }
    public int Port { get; }
}