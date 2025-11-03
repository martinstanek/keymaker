using System;

namespace Keymaker.Api.Logging;

public sealed record LogMessage
{
    public required DateTime Timestamp { get; init; }

    public required string Level { get; init; }

    public required string Message { get; init; }

    public required string Category { get; init; }

    public Exception? Exception { get; init; }

    public override string ToString()
    {
        return Exception is null
            ? $"[{Timestamp:HH:mm:ss} {Level}] {Category} - {Message}"
            : $"[{Timestamp:HH:mm:ss} {Level}] {Category} - {Message} - {Exception}";
    }
}