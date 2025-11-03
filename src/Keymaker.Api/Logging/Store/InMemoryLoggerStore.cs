using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Keymaker.Api.Logging.Store;

public sealed class InMemoryLoggerStore : IInMemoryLoggerStore
{
    private readonly ConcurrentBag<string> _messages = new();

    public IEnumerable<string> GetMessages()
    {
        return _messages;
    }


    public string GetAndJoinMessages()
    {
        return string.Join(Environment.NewLine, GetMessages());
    }

    public void Clear()
    {
        _messages.Clear();
    }

    public void AddMessage(string message)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(message);

        _messages.Add(message);
    }
}