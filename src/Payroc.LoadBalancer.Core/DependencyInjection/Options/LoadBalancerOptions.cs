namespace Payroc.LoadBalancer.Core.DependencyInjection.Options;

public sealed class LoadBalancerOptions
{
    public ServerLocation? ServerLocation { get; set; }
    public int MillisecondsBetweenRequestsIfBusy { get; set; }
}