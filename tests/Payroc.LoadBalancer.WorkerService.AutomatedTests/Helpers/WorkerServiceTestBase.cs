using Castle.DynamicProxy;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Xunit.Abstractions;

namespace Payroc.LoadBalancer.WorkerService.AutomatedTests.Helpers;

/// <summary>
/// Base class for automated and integration tests using WebApplicationFactory of TEntryPoint.
/// Provides access to the dependency injection container and access to any instance in the system.
/// Also provides an instance of IMessageBusHelper for publishing and receiving events.
/// </summary>
/// <typeparam name="TWebApplicationFactory">The web application factory for starting up
/// integration and automated tests</typeparam>
/// <typeparam name="TEntryPoint">The entry point class, typically this is Program</typeparam>
public abstract class WorkerServiceTestBase<TWebApplicationFactory, TEntryPoint> : IAsyncLifetime
    where TEntryPoint : class
    where TWebApplicationFactory : WebApplicationFactory<TEntryPoint>
{
    protected WebApplicationFactory<TEntryPoint>? ContainerApplicationFactory;

    protected WorkerServiceTestBase(ITestOutputHelper testOutputHelper)
    {
        if (Activator.CreateInstance(typeof(TWebApplicationFactory)) is not WebApplicationFactory<TEntryPoint> webApplication)
        {
            throw new ApplicationException(
                """
                 Unknown entry point or WebApplicationFactory<T> given. Ensure program is used for TEntryPoint
                 and a subclass of WebApplicationFactory is used for TWebApplicationFactory
                 """);
        }

        ContainerApplicationFactory = webApplication
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    services.AddLogging(innerBuilder =>
                        innerBuilder
                            .ClearProviders()
                            .SetMinimumLevel(LogLevel.Trace)
                            .AddProvider(new TestLoggerProvider(testOutputHelper)));
                });
            });

        Services = ContainerApplicationFactory.Services;
        Client = Services.GetRequiredService<IHttpClientFactory>().CreateClient();
        Client.DefaultRequestHeaders.ConnectionClose = true; //Ensures connections are new
    }

    public IServiceProvider Services { get; }
    public HttpClient Client { get; set; }

    public void Dispose() =>
        ContainerApplicationFactory?.Dispose();

    public Task InitializeAsync() =>
        Task.CompletedTask;

    public async Task DisposeAsync() =>
        await DisposeContainerApplicationFactory();

    private async Task DisposeContainerApplicationFactory()
    {
        if (ContainerApplicationFactory is null)
        {
            return;
        }

        await ContainerApplicationFactory.DisposeAsync();
    }
}
