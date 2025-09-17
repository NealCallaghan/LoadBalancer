using System.Net;

namespace Payroc.LoadBalancer.Core.Backend;

public sealed record ServerMetadata(bool InUse, int TimesUsed);
public sealed record Server(IPAddress IpAddress, int Port, ServerMetadata Metadata);
public sealed record LoadBalancerServer(IPAddress IpAddress, int Port);
