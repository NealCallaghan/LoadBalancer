using System.Net.Sockets;
using Microsoft.Extensions.Logging;
using Payroc.LoadBalancer.Core.Backend;

namespace Payroc.LoadBalancer.Core.Services;

public sealed class LoadBalancer(
    ILogger<LoadBalancer> logger,
    ITrafficForwarder trafficForwarder,
    IHeathChecker healthChecker,
    LoadBalancerServer loadBalancerServer)
    : ILoadBalancer, IDisposable
{
    private readonly TcpListener _tcpListener = new(loadBalancerServer.IpAddress, loadBalancerServer.Port);
    private CancellationTokenSource? _cancellationTokenSource;
    private Task? _loadBalancerTask;
    private Task? _healthCheckTask;

    public Task Start(CancellationToken cancellationToken)
    {
        _tcpListener.Start();
        _cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        
        logger.LogInformation("Started TcpListener at: {TimeNow}", DateTime.UtcNow);
        
        _loadBalancerTask = Task.Run(async () => await PerformTcpListening(), _cancellationTokenSource.Token);
        _healthCheckTask = Task.Run(async () => await healthChecker.Initialize(_cancellationTokenSource.Token), _cancellationTokenSource.Token);
        
        return Task.CompletedTask;
    }

    public async Task Stop()
    {
        logger.LogInformation("Stopping TcpListener...");
        
        await _cancellationTokenSource?.CancelAsync()!;
        _tcpListener.Stop();

        if (_loadBalancerTask != null)
        {
            try
            {
                await _loadBalancerTask;
            }
            catch (OperationCanceledException)
            {
                logger.LogInformation("Stopped TcpListener at: {TimeNow}", DateTime.UtcNow);
            }
        }
        
        if (_healthCheckTask != null)
        {
            try
            {
                await _healthCheckTask;
            }
            catch (OperationCanceledException)
            {
                logger.LogInformation("Stopped Health Checker at: {TimeNow}", DateTime.UtcNow);
            }
        }
    }

    public void Dispose()
    {
        _cancellationTokenSource?.Dispose();
        _tcpListener?.Dispose();
    }

    private async Task PerformTcpListening()
    {
        if (_cancellationTokenSource is null)
        {
            return;
        }
        
        try
        {
            while (!_cancellationTokenSource.Token.IsCancellationRequested)
            {
                try
                {
                    var client = await _tcpListener.AcceptTcpClientAsync(_cancellationTokenSource.Token);
                    await trafficForwarder.HandleForwarding(client, _cancellationTokenSource.Token);
                }
                catch (OperationCanceledException)
                {
                    logger.LogDebug("Listener loop has been cancelled, breaking loop: {TimeNow}", DateTime.UtcNow);
                    break;
                }
                    
                logger.LogDebug("Accepted TcpClient at: {TimeNow}", DateTime.UtcNow);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unexpected exception in listener");
        }
    }
}