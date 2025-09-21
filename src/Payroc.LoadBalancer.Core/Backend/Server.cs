using System.Collections.Concurrent;
using System.Net;

namespace Payroc.LoadBalancer.Core.Backend;

public sealed record ServerState(int TimesUsed, bool Healthy);
public sealed record ServerAddressAndPort(string Address, int Port);
public sealed record Server(ServerAddressAndPort ServerAddressAndPort, ServerState State);
public sealed record LoadBalancerServer(IPAddress IpAddress, int Port);

public sealed record ClusterState(ConcurrentDictionary<ServerAddressAndPort, ServerState> ServerDictionary);
