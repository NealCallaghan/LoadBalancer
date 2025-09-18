using System.Collections.Concurrent;
using System.Net;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Payroc.LoadBalancer.Core.Backend;
using Payroc.LoadBalancer.Core.DependencyInjection.Options;
using Payroc.LoadBalancer.Core.Exceptions;

namespace Payroc.LoadBalancer.Core.UnitTests;

public class ServerProviderTests
{
    private readonly ILogger<ServerProvider> _logger = Mock.Of<ILogger<ServerProvider>>();
    private readonly IPAddress _ip = IPAddress.Parse("127.0.0.1");

    private readonly LoadBalancerOptions _options = new LoadBalancerOptions
    {
        MillisecondsBetweenRequestsIfBusy = 50
    };
    
    private static ConcurrentDictionary<Server, Server> CreateServerDictionary(params Server[] servers)
    {
        var dict = new ConcurrentDictionary<Server, Server>();
        foreach (var s in servers.OrderBy(x => x.Port))
        {
            dict.TryAdd(s, s);
        }
        return dict;
    }
    
    [Fact]
    public void Constructor_Throws_WhenNoServersConfigured()
    {
        var emptyServers = new ConcurrentDictionary<Server, Server>();
        var clusterState = new ClusterState(emptyServers);

        var action = () => new ServerProvider(_logger, clusterState, _options);

        action.Should().Throw<NoAvailableServersException>().WithMessage("No servers configured");
    }

    [Fact]
    public async Task GetNextServer_ReturnsFirstServer()
    {
        var server1 = new Server(_ip, 1, new ServerState(0, true));
        var server2 = new Server(_ip, 2, new ServerState(0, true));
        var servers = CreateServerDictionary(server1, server2);
        var clusterState = new ClusterState(servers);

        var provider = new ServerProvider(_logger, clusterState, _options);

        var result = await provider.GetNextServer(CancellationToken.None);

        result.Should().Be(server1);
    }
    
    [Fact]
    public async Task SetServerUsed_IncrementsUsageCount()
    {
        var server1 = new Server(_ip, 1, new ServerState(0, true));
        var server2 = new Server(_ip, 2, new ServerState(0, true));
        var servers = CreateServerDictionary(server1, server2);
        var clusterState = new ClusterState(servers);

        var provider = new ServerProvider(_logger, clusterState, _options);

        var selected = await provider.GetNextServer(CancellationToken.None);
        provider.SetServerUsed(selected);

        // Fetch the updated value from the dictionary
        servers.TryGetValue(selected, out var updatedServer).Should().BeTrue();
        updatedServer!.State.TimesUsed.Should().Be(1);
    }
    
    [Fact]
    public async Task GetNextServer_SkipsUnhealthyServers()
    {
        var unhealthy = new Server(_ip, 1, new ServerState(0, false));
        var healthy = new Server(_ip, 2, new ServerState(0, true));

        var servers = CreateServerDictionary(unhealthy, healthy);
        var clusterState = new ClusterState(servers);

        var provider = new ServerProvider(_logger, clusterState, _options);

        var result = await provider.GetNextServer(CancellationToken.None);

        result.Should().Be(healthy);
    }
    
    [Fact]
    public async Task SetServerUsed_ReturnsSecondServer()
    {
        var server1 = new Server(_ip, 1, new ServerState(0, true));
        var server2 = new Server(_ip, 2, new ServerState(0, true));
        
        var servers = CreateServerDictionary(server1, server2);
        var clusterState = new ClusterState(servers);
    
        var provider = new ServerProvider(_logger, clusterState, _options);
    
        var potentialServer = await provider.GetNextServer(CancellationToken.None);
        provider.SetServerUsed(potentialServer);
    
        var result = await provider.GetNextServer(CancellationToken.None);
    
        result.Should().Be(server2);
    }
}