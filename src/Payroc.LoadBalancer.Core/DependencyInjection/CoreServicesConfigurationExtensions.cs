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
        serviceCollection.AddSingleton<ITrafficForwarder, TrafficForwarder>();
        serviceCollection.AddSingleton<ILoadBalancer, Services.LoadBalancer>();
        
        return serviceCollection;
    }

    private static void RegisterOptionsFromConfiguration(
        this IServiceCollection serviceCollection,
        IConfiguration configuration)
    {
        LoadBalancerOptions loadBalancerOptions = new();
        configuration.GetSection(nameof(LoadBalancerOptions)).Bind(loadBalancerOptions);
        
        var (serverAddress, serverPort) = ValidateOptionsOrThrow(loadBalancerOptions.ServerLocation!);
        var loadBalancerServer = new LoadBalancerServer(serverAddress, serverPort);
        serviceCollection.AddSingleton(loadBalancerServer);
        
        ServerProviderOptions serverProviderOptions = new();
        configuration.GetSection(nameof(ServerProviderOptions)).Bind(serverProviderOptions);
        
        var servers = serverProviderOptions.Servers.Select(server =>
        {
            var (address, port) = ValidateOptionsOrThrow(server);
            return new Server(address, port, new ServerMetadata(true, 0));
        }).ToList();
        
        serviceCollection.AddSingleton<IReadOnlyCollection<Server>>(servers);
    }
}
