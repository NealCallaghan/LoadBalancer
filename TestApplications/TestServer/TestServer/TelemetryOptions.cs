using System.Security.Authentication.ExtendedProtection;

namespace TestServer;

public class TelemetryOptions
{
    public string? ServiceName { get; set; }
    public string? ExportEndpoint { get; set; }
}