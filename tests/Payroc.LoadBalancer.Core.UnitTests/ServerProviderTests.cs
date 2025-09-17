using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Payroc.LoadBalancer.Core.Backend;

namespace Payroc.LoadBalancer.Core.UnitTests;

public class ServerProviderTests
{
    private readonly ILogger<ServerProvider> _logger = Mock.Of<ILogger<ServerProvider>>();
    
    [Fact]
    public void Constructor_Throws_WhenNoServersConfigured()
    {
        var servers = Array.Empty<IServer>();
        
        var action = () => new ServerProvider(_logger, servers);
        action.Should().Throw<ApplicationException>().WithMessage("No servers configured");
    }

    [Fact]
    public void GetNextServer_ReturnsFirstServer()
    {
        var server1 = Mock.Of<IServer>();
        var server2 = Mock.Of<IServer>();
        var servers = new List<IServer> { server1, server2 };

        var provider = new ServerProvider(_logger, servers);
        
        var result = provider.GetNextServer();
        
        result.Should().Be(server1);
    }
}