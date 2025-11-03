using System.Collections.Concurrent;
using Keymaker.Api.Logging.Store;
using Microsoft.Extensions.Logging;

namespace Keymaker.Api.Logging;

[ProviderAlias("InMemory")]
public class InMemoryLoggerProvider : ILoggerProvider
{
    private readonly IInMemoryLoggerStore _store;
    private readonly ConcurrentDictionary<string, InMemoryLogger> _loggers = new();

    public InMemoryLoggerProvider(IInMemoryLoggerStore store)
    {
        _store = store;
    }

    public ILogger CreateLogger(string categoryName)
    {
        return _loggers.GetOrAdd(categoryName, name => new InMemoryLogger(name, _store));
    }

    public void Dispose()
    {
        _loggers.Clear();
    }
}