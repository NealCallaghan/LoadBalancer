namespace Payroc.LoadBalancer.Core.Services;

public interface ILoadBalancer
{
    Task Start(CancellationToken cancellationToken);
    Task Stop();
}