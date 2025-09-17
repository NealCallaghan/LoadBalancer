namespace Payroc.LoadBalancer.Core.DependencyInjection.Options;

public sealed class ServerLocation
{
    public string? Address { get; set; }
    public int Port { get; set; }
}