using System.Net.Sockets;

namespace Payroc.LoadBalancer.Core.Services;

public interface ITrafficForwarder
{
    Task HandleForwarding(TcpClient client, CancellationToken cancellationToken);
}