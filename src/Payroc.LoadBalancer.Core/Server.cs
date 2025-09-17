using System.Net;

namespace Payroc.LoadBalancer.Core;

public sealed record Server(IPAddress IpAddress, int Port) : IServer;