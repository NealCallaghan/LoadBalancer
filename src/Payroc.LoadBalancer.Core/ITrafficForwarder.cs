using System.Net.Sockets;

namespace Payroc.LoadBalancer.Core;

public interface ITrafficForwarder
{
    Task HandleForwarding(TcpClient client, CancellationToken cancellationToken);
}