using System;
using Keymaker.Api.Logging.Store;
using Microsoft.Extensions.Logging;

namespace Keymaker.Api.Logging;

public sealed class InMemoryLogger : ILogger
{
    private readonly string _name;
    private readonly IInMemoryLoggerStore _store;

    public InMemoryLogger(string name, IInMemoryLoggerStore store)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        _name = name;
        _store = store;
    }

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        if (!IsEnabled(logLevel))
        {
            return;
        }
        var logMessage = new LogMessage
        {
            Timestamp = DateTime.Now,
            Level = logLevel.ToString(),
            Message = formatter(state, exception),
            Category = _name
        };

        _store.AddMessage(logMessage);
    }

    public bool IsEnabled(LogLevel logLevel)
    {
        return logLevel != LogLevel.None;
    }

    public IDisposable BeginScope<TState>(TState state) where TState : notnull
    {
        return NullScope.Instance;
    }

    private class NullScope : IDisposable
    {
        public static NullScope Instance { get; } = new();

        private NullScope() { }

        public void Dispose() { }
    }
}