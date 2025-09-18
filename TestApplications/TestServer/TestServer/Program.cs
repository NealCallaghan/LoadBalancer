using System.Net;
using Microsoft.AspNetCore.Mvc;
using OpenTelemetry.Logs;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using TestServer;

var builder = WebApplication.CreateBuilder(args);

TelemetryOptions telemetryOptions = new();
builder.Configuration.GetSection(nameof(TelemetryOptions)).Bind(telemetryOptions);

var endPoint = new Uri(telemetryOptions.ExportEndpoint!);

builder.Services.AddOpenTelemetry()
    .ConfigureResource(resource => resource.AddService(telemetryOptions.ServiceName!))
    .WithTracing(tracing =>
    {
        tracing
            .AddAspNetCoreInstrumentation()
            .AddHttpClientInstrumentation();

        tracing.AddOtlpExporter(exporterOptions =>
            exporterOptions.Endpoint = endPoint);
    })
    .WithLogging(logging =>
        logging.AddOtlpExporter(exporterOptions =>
            exporterOptions.Endpoint = endPoint));

builder.Logging.ClearProviders();
builder.Logging.AddConsole();

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

//app.UseHttpsRedirection();

var serverHealthy = true;

app.MapGet("/", (ILogger<Program> logger) =>
{
    logger.LogInformation("Received request on / endpoint. Healthy = {Healthy}", serverHealthy);
    
    var serverId = Environment.GetEnvironmentVariable("ServerId") ?? "Unknown";
    
    return serverHealthy
        ? Results.Text($"Hello from {serverId}")
        : Results.StatusCode((int)HttpStatusCode.InternalServerError);
});


app.MapGet("/health", (ILogger<Program> logger) =>
{
    logger.LogInformation("Health check requested. Healthy = {Healthy}", serverHealthy);
    
    return serverHealthy
        ? Results.Text("Healthy")
        : Results.StatusCode((int)HttpStatusCode.ServiceUnavailable);
});

// Endpoint to simulate failure
app.MapPost("/toggle-health", ([FromBody]bool health, ILogger<Program> logger) =>
{
    serverHealthy = health;
    logger.LogWarning("Server health manually toggled. Now healthy = {Healthy}", serverHealthy);
    return Results.Ok($"Server health toggled. Now healthy: {serverHealthy}");
});

app.Run();