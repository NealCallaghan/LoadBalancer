using System.Collections.Concurrent;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Payroc.LoadBalancer.Core.Backend;
using Payroc.LoadBalancer.Core.DependencyInjection.Options;
using Payroc.LoadBalancer.Core.Services;

namespace Payroc.LoadBalancer.Core.DependencyInjection;

using static OptionsHelper;

public static class CoreServicesConfigurationExtensions
{
    public static IServiceCollection RegisterCoreServices(
        this IServiceCollection serviceCollection, IConfiguration configuration)
    {
        serviceCollection.RegisterOptionsFromConfiguration(configuration);

        serviceCollection.AddSingleton<IServerProvider, ServerProvider>();
        serviceCollection.AddSingleton<IServerUpdater, ServerProvider>();
        serviceCollection.AddSingleton<ITrafficForwarder, TrafficForwarder>();
        serviceCollection.AddSingleton<ILoadBalancer, Services.LoadBalancer>();
        serviceCollection.AddSingleton<IHeathChecker, HealthChecker>();
        
        return serviceCollection;
    }

    private static void RegisterOptionsFromConfiguration(
        this IServiceCollection serviceCollection,
        IConfiguration configuration)
    {
        HealthServiceOptions healthServiceOptions = new();
        configuration.GetSection(nameof(HealthServiceOptions)).Bind(healthServiceOptions);
        serviceCollection.AddSingleton(healthServiceOptions);
        
        LoadBalancerOptions loadBalancerOptions = new();
        configuration.GetSection(nameof(LoadBalancerOptions)).Bind(loadBalancerOptions);
        serviceCollection.AddSingleton(loadBalancerOptions);
        
        var (serverAddress, serverPort) = ValidateOptionsOrThrow(loadBalancerOptions.ServerLocation!);
        var loadBalancerServer = new LoadBalancerServer(serverAddress, serverPort);
        serviceCollection.AddSingleton(loadBalancerServer);
        
        ServerProviderOptions serverProviderOptions = new();
        configuration.GetSection(nameof(ServerProviderOptions)).Bind(serverProviderOptions);
        
        var servers = serverProviderOptions
            .Servers
            .Select(server => 
                new Server(new ServerAddressAndPort(server.Address!, server.Port), new ServerState(0, true)));
        
        var concurrentDictionary = new ConcurrentDictionary<ServerAddressAndPort, ServerState>(
            servers.Select(x => new KeyValuePair<ServerAddressAndPort, ServerState>(x.ServerAddressAndPort, x.State)));
        
        var clusterState = new ClusterState(concurrentDictionary);
        serviceCollection.AddSingleton(clusterState);
    }
}
