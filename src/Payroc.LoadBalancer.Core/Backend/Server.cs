using System.Net;

namespace Payroc.LoadBalancer.Core.Backend;

public sealed record Server(IPAddress IpAddress, int Port) : IServer;