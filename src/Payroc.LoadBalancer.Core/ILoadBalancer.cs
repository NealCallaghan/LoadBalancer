namespace Payroc.LoadBalancer.Core;

public interface ILoadBalancer
{
    Task Start(CancellationToken cancellationToken);
    Task Stop();
}