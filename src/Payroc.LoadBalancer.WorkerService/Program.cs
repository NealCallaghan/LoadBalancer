using Payroc.LoadBalancer.Core.DependencyInjection;
using Payroc.LoadBalancer.WorkerService;
using Payroc.LoadBalancer.WorkerService.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

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
