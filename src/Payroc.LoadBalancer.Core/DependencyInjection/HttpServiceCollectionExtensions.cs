using Microsoft.Extensions.DependencyInjection;

namespace Payroc.LoadBalancer.Core.DependencyInjection;

public static class HttpServiceCollectionExtensions
{
    public static IServiceCollection AddHttpServices(this IServiceCollection services)
    {
        services.AddHttpClient();
        
        return services;
    }
}