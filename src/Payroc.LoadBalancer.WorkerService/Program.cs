using Payroc.LoadBalancer.Core.DependencyInjection;
using Payroc.LoadBalancer.WorkerService;
using Payroc.LoadBalancer.WorkerService.DependencyInjection;

var builder = Host.CreateApplicationBuilder(args);

builder.Logging.AddConsole();

var configuration = builder.Configuration;
var services = builder.Services;
services
    .ConfigureHostOptions(configuration)
    .RegisterCoreServices(configuration)
    .AddHostedService<Worker>();

var host = builder.Build();
host.Run();

// Required for accessibility
public partial class Program;
