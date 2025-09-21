using System.Collections.Concurrent;
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

    private readonly LoadBalancerOptions _options = new LoadBalancerOptions
    {
        MillisecondsBetweenRequestsIfBusy = 50
    };

    private static ConcurrentDictionary<ServerAddressAndPort, ServerState> CreateServerDictionary(params Server[] servers)
    {
        var dict = new ConcurrentDictionary<ServerAddressAndPort, ServerState>();
        foreach (var s in servers.OrderBy(x => x.ServerAddressAndPort.Port))
        {
            dict.TryAdd(s.ServerAddressAndPort, s.State);
        }
        return dict;
    }
    
    [Fact]
    public void Constructor_Throws_WhenNoServersConfigured()
    {
        var emptyServers = new ConcurrentDictionary<ServerAddressAndPort, ServerState>();
        var clusterState = new ClusterState(emptyServers);

        var action = () => new ServerProvider(_logger, clusterState, _options);

        action.Should().Throw<NoAvailableServersException>().WithMessage("No servers configured");
    }
    
    [Fact]
    public async Task GetNextServer_ReturnsAServer()
    {
        var server1 = new Server(new ServerAddressAndPort("127.0.0.1", 1), new ServerState(0, true));
        
        var servers = CreateServerDictionary(server1);
        var clusterState = new ClusterState(servers);
    
        var provider = new ServerProvider(_logger, clusterState, _options);
    
        var result = await provider.GetNextServer(CancellationToken.None);
    
        result.Should().Be(server1.ServerAddressAndPort);
    }
    
    [Fact]
    public async Task SetServerUsed_IncrementsUsageCount()
    {
        var server1 = new Server(new ServerAddressAndPort("127.0.0.1", 1), new ServerState(0, true));
        var server2 = new Server(new ServerAddressAndPort("127.0.0.1", 2), new ServerState(0, true));
        var servers = CreateServerDictionary(server1, server2);
        var clusterState = new ClusterState(servers);
    
        var provider = new ServerProvider(_logger, clusterState, _options);
    
        var selected = await provider.GetNextServer(CancellationToken.None);
        provider.SetServerUsed(selected);
    
        // Fetch the updated value from the dictionary
        servers.TryGetValue(selected, out var updatedServer).Should().BeTrue();
        updatedServer!.TimesUsed.Should().Be(1);
    }
    
    [Fact]
    public async Task GetNextServer_SkipsUnhealthyServers()
    {
        var unhealthy = new Server(new ServerAddressAndPort("127.0.0.1", 1), new ServerState(0, false));
        var healthy = new Server(new ServerAddressAndPort("127.0.0.1", 2), new ServerState(0, true));
    
        var servers = CreateServerDictionary(unhealthy, healthy);
        var clusterState = new ClusterState(servers);
    
        var provider = new ServerProvider(_logger, clusterState, _options);
    
        var result = await provider.GetNextServer(CancellationToken.None);
    
        result.Should().Be(healthy.ServerAddressAndPort);
    }
    
    [Fact]
    public async Task SetServerUsed_ReturnsSecondServer()
    {
        var server1 = new Server(new ServerAddressAndPort("127.0.0.1", 1), new ServerState(0, true));
        var server2 = new Server(new ServerAddressAndPort("127.0.0.1", 2), new ServerState(0, true));
        
        var servers = CreateServerDictionary(server1, server2);
        var clusterState = new ClusterState(servers);
    
        var provider = new ServerProvider(_logger, clusterState, _options);
    
        var potentialServer = await provider.GetNextServer(CancellationToken.None);
        provider.SetServerUsed(potentialServer);
    
        var result = await provider.GetNextServer(CancellationToken.None);
    
        result.Should().NotBe(potentialServer);
    }
}