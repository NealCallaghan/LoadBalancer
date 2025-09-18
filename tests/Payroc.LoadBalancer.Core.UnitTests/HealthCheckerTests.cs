using System.Net;
using System.Collections.Concurrent;
using System.Net.Http;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Payroc.LoadBalancer.Core.Backend;
using Payroc.LoadBalancer.Core.DependencyInjection.Options;
using Payroc.LoadBalancer.Core.Services;

namespace Payroc.LoadBalancer.Core.UnitTests;

public class HealthCheckerTests
{
    private readonly Mock<ILogger<HealthChecker>> _logger = new();
    private readonly HealthServiceOptions _options = new() { HealthServiceDelayInSeconds = 1 };

    private static HttpClient CreateHttpClientReturning(HttpStatusCode statusCode, string? content = null)
    {
        var response = new HttpResponseMessage(statusCode)
        {
            Content = content is not null ? new StringContent(content) : null
        };
        var handler = new StubHttpMessageHandler(response);
        return new HttpClient(handler);
    }

    [Fact]
    public async Task Initialize_MarksServerUnhealthy_WhenRequestFails()
    {
        // Arrange
        var server = new Server(IPAddress.Loopback, 8080, new ServerState(TimesUsed: 0, Healthy: true));
        var serverDictionary = new ConcurrentDictionary<Server, Server>
        {
            [server] = server
        };

        var clusterState = new ClusterState(serverDictionary);
        // Return 500 with empty content
        var httpClient = CreateHttpClientReturning(HttpStatusCode.InternalServerError);

        var factoryMock = new Mock<IHttpClientFactory>();
        factoryMock.Setup(f => f.CreateClient(It.IsAny<string>())).Returns(httpClient);

        var checker = new HealthChecker(_logger.Object, clusterState, factoryMock.Object, _options);

        using var cts = new CancellationTokenSource();
        cts.CancelAfter(TimeSpan.FromMilliseconds(200)); // force only one loop iteration

        var action = async () => await checker.Initialize(cts.Token);
        await action.Should().ThrowAsync<OperationCanceledException>();

        clusterState.ServerDictionary.Values.Should().ContainSingle();
        var updatedServer = clusterState.ServerDictionary.Values.First();
        updatedServer.State.Healthy.Should().BeFalse(); // should now be unhealthy
    }

    [Fact]
    public async Task Initialize_LeavesServerHealthy_WhenRequestSucceeds()
    {
        // Arrange
        var server = new Server(IPAddress.Loopback, 8080, new ServerState(TimesUsed: 0, Healthy: true));
        var serverDictionary = new ConcurrentDictionary<Server, Server>
        {
            [server] = server
        };

        var clusterState = new ClusterState(serverDictionary);
        // Return 200 with "Healthy" content
        var httpClient = CreateHttpClientReturning(HttpStatusCode.OK, "Healthy");

        var factoryMock = new Mock<IHttpClientFactory>();
        factoryMock.Setup(f => f.CreateClient(It.IsAny<string>())).Returns(httpClient);

        var checker = new HealthChecker(_logger.Object, clusterState, factoryMock.Object, _options);

        using var cts = new CancellationTokenSource();
        cts.CancelAfter(TimeSpan.FromMilliseconds(200));

        var action = async () => await checker.Initialize(cts.Token);
        await action.Should().ThrowAsync<OperationCanceledException>();

        var updatedServer = clusterState.ServerDictionary.Values.First();
        updatedServer.State.Healthy.Should().BeTrue(); // should still be healthy
    }

    private class StubHttpMessageHandler : HttpMessageHandler
    {
        private readonly HttpResponseMessage _response;

        public StubHttpMessageHandler(HttpResponseMessage response)
        {
            _response = response;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            return Task.FromResult(_response);
        }
    }
}
