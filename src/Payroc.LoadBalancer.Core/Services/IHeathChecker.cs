namespace Payroc.LoadBalancer.Core.Services;

public interface IHeathChecker
{
    Task Initialize(CancellationToken cancellationToken);
}