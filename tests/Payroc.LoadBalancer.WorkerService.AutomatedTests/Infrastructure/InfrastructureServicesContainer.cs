using Docker.DotNet;
using Docker.DotNet.Models;
using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;
using DotNet.Testcontainers.Networks;
using Payroc.LoadBalancer.WorkerService.AutomatedTests.Helpers;

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
        
        var solutionPath = CommonDirectoryPath.GetSolutionDirectory();
        var testServerImage = new ImageFromDockerfileBuilder()
            .WithDockerfileDirectory(solutionPath, $"{solutionPath.DirectoryPath}/TestApplications/TestServer/TestServer")
            .WithDockerfile("Dockerfile")
            .WithName("testserver:latest")
            .WithCleanUp(false)
            .Build();
        
        testServerImage.CreateAsync().RunSync();
        
        var testServer = new ContainerBuilder()
            .WithImage(testServerImage)
            .WithName("testserver1")
            .WithHostname("testserver1")
            .WithNetwork(_loadbalancerManagementNetwork)
            .WithPortBinding(5001, 8080) 
            .WithWaitStrategy(Wait.ForUnixContainer().UntilPortIsAvailable(8080))
            .Build();
        
        _containerNames.Add("testserver1");
        
        _containers = new List<IContainer>
        {
            testServer,
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

                var allRunningContainers = await client.Containers.ListContainersAsync(
                    new ContainersListParameters
                    {
                        All = true
                    }, cts.Token);

                var conflictingContainers = 
                    allRunningContainers.Where(x => _containerNames.Contains(x.Names.First().TrimStart('/')));

                foreach (var container in conflictingContainers)
                {
                    if (container.State == "running")
                    {
                        await client.Containers.StopContainerAsync(container.ID, new ContainerStopParameters(), cts.Token);
                    }

                    // Remove the container entirely
                    await client.Containers.RemoveContainerAsync(
                        container.ID,
                        new ContainerRemoveParameters { Force = true },
                        cts.Token);
                }
            }
            while (_containerNames.Any(containerName => runningContainers.Contains($"/{containerName}")));
        }, cts.Token);
    }
}
