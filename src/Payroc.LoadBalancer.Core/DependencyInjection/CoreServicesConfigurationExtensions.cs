using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Payroc.LoadBalancer.Core.DependencyInjection;

public static class CoreServicesConfigurationExtensions
{
    public static IServiceCollection RegisterCoreServices(
        this IServiceCollection serviceCollection, IConfiguration configuration)
    {
        //MessageBusEndpoints messageBusEndpoints = new();
        //configuration.GetRequiredSection(nameof(MessageBusEndpoints)).Bind(messageBusEndpoints);

        //serviceCollection.AddSingleton(messageBusEndpoints);

        //var consumerTypes = new MessageBusEndpoint[]
        //{
        //    new Consumer(messageBusEndpoints.ConsumeTopicName, typeof(IPageNotificationConsumer), typeof(ChequeDataExtracted)),
        //    new Publisher(messageBusEndpoints.PublishTopicName, typeof(DocumentSucceeded))
        //};

        return serviceCollection;
    }
}
