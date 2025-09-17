using System.Net;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Payroc.LoadBalancer.Core.Backend;

namespace Payroc.LoadBalancer.Core.UnitTests;

public class ServerProviderTests
{
    private readonly ILogger<ServerProvider> _logger = Mock.Of<ILogger<ServerProvider>>();
    private readonly IPAddress _ip = IPAddress.Parse("127.0.0.1");
    
    [Fact]
    public void Constructor_Throws_WhenNoServersConfigured()
    {
        var servers = Array.Empty<Server>();
        
        var action = () => new ServerProvider(_logger, servers);
        action.Should().Throw<ApplicationException>().WithMessage("No servers configured");
    }

    [Fact]
    public void GetNextServer_ReturnsFirstServer()
    {
        var server1 = Mock.Of<Server>();
        var server2 = Mock.Of<Server>();
        var servers = new List<Server> { server1, server2 };

        var provider = new ServerProvider(_logger, servers);
        
        var result = provider.GetNextServer();
        
        result.Should().Be(server1);
    }
    
    [Fact]
    public void SetServerUsed_ReturnsSecondServer()
    {
        var server1 = new Server(_ip, 1, new ServerMetadata(true, 0));
        var server2 = new Server(_ip, 2, new ServerMetadata(true, 0));
        var servers = new List<Server> { server1, server2 };
        
        var provider = new ServerProvider(_logger, servers);
        
        var potentialServer = provider.GetNextServer();
        provider.SetServerUsed(potentialServer);
        
        var result = provider.GetNextServer();
        
        result.Should().Be(server2);
    }
    
    [Fact]
    public void SetServerUsed_UpdatesServerMetadata()
    {
        var server1 = new Server(_ip, 1, new ServerMetadata(true, 0));
        var server2 = new Server(_ip, 2, new ServerMetadata(true, 0));
        var servers = new List<Server> { server1, server2 };
        
        var provider = new ServerProvider(_logger, servers);
        
        var potentialServer = provider.GetNextServer();
        provider.SetServerUsed(potentialServer);
        
        server1.Should().Be(potentialServer);
        server1.Metadata.Should().BeEquivalentTo(new ServerMetadata(true, 1));
    }
    
    [Fact]
    public void SetServerAsUnresponsive_ReturnsSecondServer()
    {
        var server1 = new Server(_ip, 1, new ServerMetadata(true, 0));
        var server2 = new Server(_ip, 2, new ServerMetadata(true, 0));
        var servers = new List<Server> { server1, server2 };
        
        var provider = new ServerProvider(_logger, servers);
        
        var potentialServer = provider.GetNextServer();
        provider.SetServerAsUnresponsive(potentialServer);
        
        server1.Should().Be(potentialServer);
        server1.Metadata.Should().BeEquivalentTo(new ServerMetadata(false, 0));
    }
}