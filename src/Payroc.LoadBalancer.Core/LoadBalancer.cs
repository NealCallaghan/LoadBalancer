using System.Net;
using System.Net.Sockets;
using Microsoft.Extensions.Logging;
using Payroc.LoadBalancer.Core.DependencyInjection.Options;

namespace Payroc.LoadBalancer.Core;

public sealed class LoadBalancer : ILoadBalancer, IDisposable
{
    private readonly ILogger<LoadBalancer> _logger;
    private readonly ITrafficForwarder _trafficForwarder;
    private readonly TcpListener _tcpListener;
    private CancellationTokenSource? _cancellationTokenSource;
    private Task? _task;

    public LoadBalancer(ILogger<LoadBalancer> logger, ITrafficForwarder trafficForwarder, LoadBalancerOptions options)
    {
        _logger = logger;
        _trafficForwarder = trafficForwarder;

        var (address, port) = ValidateOptionsOrThrow(options);
        _tcpListener = new TcpListener(address, port);
    }

    public Task Start(CancellationToken cancellationToken)
    {
        _tcpListener.Start();
        _cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        
        _logger.LogInformation("Started TcpListener at: {TimeNow}", DateTime.UtcNow);

        _task = Task.Run(async () => await PerformTcpListening(), _cancellationTokenSource.Token);
        
        return Task.CompletedTask;
    }

    public async Task Stop()
    {
        _logger.LogInformation("Stopping TcpListener...");
        
        await _cancellationTokenSource?.CancelAsync()!;
        _tcpListener.Stop();

        if (_task != null)
        {
            try
            {
                await _task;
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("Stopped TcpListener at: {TimeNow}", DateTime.UtcNow);
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
                TcpClient? client = null;
                try
                {
                    client = await _tcpListener.AcceptTcpClientAsync(_cancellationTokenSource.Token);
                    await _trafficForwarder.HandleForwarding(client, _cancellationTokenSource.Token);
                }
                catch (OperationCanceledException)
                {
                    _logger.LogDebug("Listener loop has been cancelled, breaking loop: {TimeNow}", DateTime.UtcNow);
                    break;
                }
                finally
                {
                    client?.Dispose();
                }
                    
                _logger.LogDebug("Accepted TcpClient at: {TimeNow}", DateTime.UtcNow);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected exception in listener");
        }
    }

    private static (IPAddress, int) ValidateOptionsOrThrow(LoadBalancerOptions options)
    {
        var portIsValid = int.TryParse(options.Port, out var port);
        var addressIsValid = IPAddress.TryParse(options.Address, out var address);

        var errorMessages = new List<string>();
        if (!portIsValid)
        {
            errorMessages.Add($"{nameof(LoadBalancerOptions.Port)} must be an integer");
        }

        if (!addressIsValid || address is null)
        {
            errorMessages.Add($"{nameof(LoadBalancerOptions.Address)} must be a valid address");
        }

        if (errorMessages.Any())
        {
            var errorMessage = string.Join(Environment.NewLine, errorMessages);
            throw new ArgumentException(errorMessage);
        }

        return (address!, port);
    }
}