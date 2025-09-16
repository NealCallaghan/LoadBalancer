namespace Payroc.LoadBalancer.WorkerService.DependencyInjection;

public static class HostedServiceCollectionExtensions
{
    public static IServiceCollection ConfigureHostOptions(
        this IServiceCollection @this,
        IConfiguration configuration)
    {
        @this.Configure<HostOptions>(x =>
        {
            x.ServicesStartConcurrently = true;
            x.ServicesStopConcurrently = true;
            x.StartupTimeout = TimeSpan.FromSeconds(30);//get from config

            // unhandled exceptions shouldn't crash. just in case
            x.BackgroundServiceExceptionBehavior = BackgroundServiceExceptionBehavior.Ignore;
        });
        
        return @this;
    }

    public static bool IsTesting(this IHostEnvironment @this) =>
        @this.EnvironmentName == "Testing";
}