using System.Net.Sockets;
using Microsoft.Extensions.Logging;
using Payroc.LoadBalancer.Core.Backend;

namespace Payroc.LoadBalancer.Core.Services;

public class TrafficForwarder(ILogger<TrafficForwarder> logger, IServerProvider serverProvider, IServerUpdater serverUpdater)
    : ITrafficForwarder
{
    public async Task HandleForwarding(TcpClient client, CancellationToken cancellationToken)
    {
        var server = await serverProvider.GetNextServer(cancellationToken);
        using var backendClient = new TcpClient();
        
        // TODO I would like to have introduced Polly here for retries
        await backendClient.ConnectAsync(server.Address, server.Port, cancellationToken);

        logger.LogDebug("Established a connection to server on {IpAddress}:{Port} at {TimeNow}", server.Address, server.Port, DateTime.UtcNow);
        
        using (client)
        {
            await using var clientStream = client.GetStream();
            await using var backendStream = backendClient.GetStream();

            var clientBuffer = new byte[1024];
            var serverBuffer = new byte[1024];
            
            await clientStream.ReadAsync(clientBuffer, 0, clientBuffer.Length, cancellationToken);
            await backendStream.WriteAsync(clientBuffer, 0, clientBuffer.Length,  cancellationToken);
            
            await backendStream.ReadAsync(serverBuffer, 0, serverBuffer.Length, cancellationToken);
            await clientStream.WriteAsync(serverBuffer, 0, clientBuffer.Length, cancellationToken);
            
            logger.LogDebug("Successfully forwarded data at {TimeNow}", DateTime.UtcNow);
            
            client.Close();
            backendClient.Close();
            
            logger.LogDebug("Closed connections between client and server");
        }
        
        serverUpdater.SetServerUsed(server);
    }
}