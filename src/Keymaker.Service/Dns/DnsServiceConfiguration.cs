namespace Keymaker.Service.Dns;

public sealed record DnsServiceConfiguration
{
    public required string Key { get; init; }

    public required string Zone { get; init; }

    public required string Email { get; init; }
}