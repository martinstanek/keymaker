namespace Keymaker.Model;

public sealed record KeyMakerConfiguration
{
    public required DnsMode DnsMode { get; init; }

    public required StorageMode StorageMode { get; init; }
}

public enum DnsMode
{
    CloudFlare,
    Azure
}

public enum StorageMode
{
    Volume,
    KeyVault
}