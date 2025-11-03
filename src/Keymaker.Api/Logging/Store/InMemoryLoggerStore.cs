using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace Keymaker.Api.Logging.Store;

public sealed class InMemoryLoggerStore : IInMemoryLoggerStore
{
    private readonly ConcurrentBag<LogMessage> _messages = new();

    public IEnumerable<string> GetMessages()
    {
        return _messages.OrderBy(o => o.Timestamp).Select(s => s.ToString());
    }


    public string GetAndJoinMessages()
    {
        return string.Join(Environment.NewLine, GetMessages());
    }

    public void Clear()
    {
        _messages.Clear();
    }

    public void AddMessage(LogMessage message)
    {
        _messages.Add(message);
    }
}