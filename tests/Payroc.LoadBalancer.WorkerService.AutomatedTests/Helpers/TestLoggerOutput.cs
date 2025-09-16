using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using Xunit.Abstractions;

namespace Payroc.LoadBalancer.WorkerService.AutomatedTests.Helpers;

public class TestLoggerOutput : ILogger
{
    private readonly ITestOutputHelper _output;
    private static readonly ConcurrentQueue<string> LogMessages = new ConcurrentQueue<string>();

    public TestLoggerOutput(ITestOutputHelper output)
    {
        _output = output;
    }

    public IDisposable? BeginScope<TState>(TState state) where TState : notnull
    {
        return null;
    }

    public bool IsEnabled(LogLevel logLevel)
    {
        return true;
    }

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        var message = formatter(state, exception);
        LogMessages.Enqueue(message);
        _output.WriteLine(message);
    }

    public static void Clear() => LogMessages.Clear();
    public static string[] GetMessages() => LogMessages.ToArray();
}

public class TestLoggerProvider : ILoggerProvider
{
    private readonly ITestOutputHelper _output;

    public TestLoggerProvider(ITestOutputHelper output)
    {
        _output = output;
    }

    public ILogger CreateLogger(string categoryName)
    {
        return new TestLoggerOutput(_output);
    }

    public void Dispose() { }
}
