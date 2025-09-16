using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Hosting;

namespace Payroc.LoadBalancer.WorkerService.AutomatedTests.Infrastructure;

public class LoadBalancerWebApplicationFactory : WebApplicationFactory <Program>
{
    protected override IHost CreateHost(IHostBuilder builder)
    {
        //We haven't created a web app but WebApplicationFactory still needs one.
        //Give it defaults.
        builder.ConfigureWebHost(b => b.Configure(_ => { }));

        return base.CreateHost(builder);
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureAppConfiguration((_, config) =>
        {
            //config.Build().GetRequiredSection(nameof(MessageBusEndpoints)).Bind(messageBusEndpoints);
        });

        builder.ConfigureTestServices(services =>
        {
            //TODO we still need these for the time being.
            //services.AddSingleton<IInterceptor, PageNotificationCompletionInterceptor>();

            //var consumerTypes = new MessageBusEndpoint[]
            //{
            //    new Consumer(messageBusEndpoints.ConsumeTopicName!, typeof(IPageNotificationConsumer),
            //        typeof(DocumentInvalid)),
            //    new Consumer(messageBusEndpoints.PublishTopicName!, typeof(IPublishedConsumer), typeof(BatchCompleted)),
            //};

            //services.AddTestDependencies(consumerTypes, typeof(IPublishedConsumer).Assembly);
        });
    }
}
