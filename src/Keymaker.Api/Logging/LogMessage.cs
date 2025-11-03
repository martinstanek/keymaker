using System;

namespace Keymaker.Api.Logging;

public sealed record LogMessage
{
    public required DateTime Timestamp { get; init; }

    public required string Level { get; init; }

    public required string Message { get; init; }

    public required string Category { get; init; }

    public override string ToString()
    {
        return $"[{Timestamp:yy.MM.dd HH:mm:ss} {Level}] {Category} - {Message}";
    }
}