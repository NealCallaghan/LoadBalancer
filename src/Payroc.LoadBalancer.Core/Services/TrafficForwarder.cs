using System.Net.Sockets;
using Microsoft.Extensions.Logging;
using Payroc.LoadBalancer.Core.Backend;

namespace Payroc.LoadBalancer.Core.Services;

public class TrafficForwarder(ILogger<TrafficForwarder> logger, IServerProvider serverProvider)
    : ITrafficForwarder
{
    public async Task HandleForwarding(TcpClient client, CancellationToken cancellationToken)
    {
        var server = serverProvider.GetNextServer();
        using var backendClient = new TcpClient();
        
        // TODO what happens if there is an exception here
        await backendClient.ConnectAsync(server.IpAddress, server.Port, cancellationToken);
        logger.LogDebug("Established a connection to server on {IpAddress}:{Port} at {TimeNow}", server.IpAddress, server.Port, DateTime.UtcNow);
        
        using (client)
        {
            await using var clientStream = client.GetStream();
            await using var backendStream = backendClient.GetStream();
            
            var clientToBackend = clientStream.CopyToAsync(backendStream, cancellationToken);
            var backendToClient = backendStream.CopyToAsync(clientStream, cancellationToken);
            
            // we want to wait for both sides to flush their data
            await Task.WhenAll(clientToBackend, backendToClient);
            logger.LogDebug("Successfully forwarded data at {TimeNow}", DateTime.UtcNow);
            
            client.Close();
            backendClient.Close();
            
            logger.LogDebug("Closed connections between client and server");
        }
    }
}