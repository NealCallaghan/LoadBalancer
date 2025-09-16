using Docker.DotNet;
using Docker.DotNet.Models;
using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;
using DotNet.Testcontainers.Networks;

namespace Payroc.LoadBalancer.WorkerService.AutomatedTests.Infrastructure;

public class InfrastructureServicesContainer : IAsyncLifetime
{
    private readonly INetwork _loadbalancerManagementNetwork;
    private readonly List<IContainer> _containers;
    private readonly HashSet<string> _containerNames = new();

    public InfrastructureServicesContainer()
    {
        _loadbalancerManagementNetwork = new NetworkBuilder()
            .WithName(Guid.NewGuid().ToString("D"))
            .Build();

        

        _containers = new List<IContainer>
        {

        };
    }

    public async Task InitializeAsync()
    {
        await EnsurePreviousTestContainersAreCleanedUp();

        await _loadbalancerManagementNetwork.CreateAsync();

        foreach (var container in _containers)
        {
            await container.StartAsync();
        }
    }

    public async Task DisposeAsync()
    {
        foreach (var container in _containers)
        {
            await container.DisposeAsync().ConfigureAwait(false);
        }

        await _loadbalancerManagementNetwork.DeleteAsync().ConfigureAwait(false);
    }

    private async Task EnsurePreviousTestContainersAreCleanedUp()
    {
        var client = new DockerClientConfiguration().CreateClient();
        var cts = new CancellationTokenSource(TimeSpan.FromSeconds(40));

        await Task.Run(async () =>
        {
            HashSet<string> runningContainers;
            do
            {
                if (cts.Token.IsCancellationRequested)
                {
                    throw new TaskCanceledException("Waiting for conflicting containers to shut down timed out");
                }

                runningContainers = (await client.Containers.ListContainersAsync(
                    new ContainersListParameters
                    {
                        All = true
                    }, cts.Token)).SelectMany(x => x.Names).ToHashSet();
            }
            while (_containerNames.Any(containerName => runningContainers.Contains($"/{containerName}")));
        }, cts.Token);
    }
}
