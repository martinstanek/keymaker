using System.Collections.Generic;

namespace Keymaker.Api.Logging.Store;

public interface IInMemoryLoggerStore
{
    IEnumerable<string> GetMessages();

    string GetAndJoinMessages();

    void Clear();

    void AddMessage(LogMessage message);
}